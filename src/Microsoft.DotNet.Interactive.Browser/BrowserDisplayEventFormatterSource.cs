// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Formatting;
using static Polyglossy.Interactive.Formatting.PocketViewTags;

namespace Polyglossy.Interactive.Browser;

internal class BrowserDisplayEventFormatterSource :
    ITypeFormatterSource,
    ITypeFormatter<BrowserDisplayEvent>
{
    public IEnumerable<ITypeFormatter> CreateTypeFormatters()
    {
        yield return this;
    }

    public string MimeType => HtmlFormatter.MimeType;

    public Type Type => typeof(BrowserDisplayEvent);

    public bool Format(object instance, FormatContext context)
    {
        return Format((BrowserDisplayEvent)instance, context);
    }

    public bool Format(BrowserDisplayEvent browserDisplayEvent, FormatContext context)
    {
        var @event = browserDisplayEvent.DisplayEvent;

        var formattedValue = @event.FormattedValues.FirstOrDefault(v => v.MimeType == HtmlFormatter.MimeType) ??
                             @event.FormattedValues.FirstOrDefault(v => v.MimeType == PlainTextFormatter.MimeType) ??
                             @event.FormattedValues.FirstOrDefault();

        if (formattedValue!.SuppressDisplay)
        {
            return false;
        }

        IHtmlContent html = formattedValue.MimeType switch
        {
            HtmlFormatter.MimeType => new HtmlString(formattedValue.Value),
            _ => code(formattedValue.Value)
        };

        var codeSummary = browserDisplayEvent.DisplayEvent.Command is SubmitCode submitCode
                              ? submitCode.ToString().Substring("SubmitCode: ".Length)
                              : "";


        html = div[@class: "dni-treeview"](
            table(
                tr(
                    td($"[{browserDisplayEvent.ExecutionOrder}]"),
                    td(
                        details[open: ""](
                            summary(
                                code(codeSummary)),
                            div(html))))),
            hr
        );

        html.WriteTo(context.Writer, HtmlEncoder.Default);

        return true;
    }
}