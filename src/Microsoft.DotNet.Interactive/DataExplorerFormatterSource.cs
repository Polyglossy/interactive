// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Text.Json;

using Polyglossy.Interactive.Formatting;
using Polyglossy.Interactive.Formatting.Csv;
using Polyglossy.Interactive.Formatting.TabularData;

namespace Polyglossy.Interactive;

internal class DataExplorerFormatterSource : ITypeFormatterSource
{
    public IEnumerable<ITypeFormatter> CreateTypeFormatters()
    {
        yield return new JsonFormatter<DataExplorer<TabularDataResource>>((dataExplorer, context) =>
        {
            var json = JsonSerializer.Serialize(dataExplorer.Data.Data,
                TabularDataResourceFormatter.JsonSerializerOptions);

            context.Writer.Write(json);
        });

        yield return new TabularDataResourceFormatter<DataExplorer<TabularDataResource>>((dataExplorer, context) =>
        {
            var json = JsonSerializer.Serialize(dataExplorer.Data, TabularDataResourceFormatter.JsonSerializerOptions);

            context.Writer.Write(json);
        });

        yield return new CsvFormatter<DataExplorer<TabularDataResource>>((dataExplorer, context) =>
        {
            dataExplorer.Data.FormatTo(context, CsvFormatter.MimeType);

            return true;
        });
    }
}