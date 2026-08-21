// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;
using Polyglossy.Interactive.Formatting;

namespace Polyglossy.Interactive;

[TypeFormatterSource(typeof(ScriptContentFormatterSource))]
public class ScriptContent : IHtmlContent
{
    public string ScriptValue { get; }

    public ScriptContent(string scriptValue)
    {
        ScriptValue = scriptValue;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        writer.Write(ScriptValue);
    }
}