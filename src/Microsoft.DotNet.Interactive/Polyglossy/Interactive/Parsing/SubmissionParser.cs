// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace Polyglossy.Interactive.Parsing;

public class SubmissionParser : Microsoft.DotNet.Interactive.Parsing.SubmissionParser
{
    public SubmissionParser(Polyglossy.Interactive.Kernel kernel)
        : base(kernel)
    {
    }
}
