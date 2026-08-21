// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace Microsoft.DotNet.Interactive.Parsing;

public class SubmissionParser : Polyglossy.Interactive.Parsing.SubmissionParser
{
    public SubmissionParser(Polyglossy.Interactive.Kernel kernel)
        : base(kernel)
    {
    }
}
