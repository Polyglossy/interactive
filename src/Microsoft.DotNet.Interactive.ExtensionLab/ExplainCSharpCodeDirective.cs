// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polyglossy.Interactive.Directives;

namespace Polyglossy.Interactive.ExtensionLab;

public class ExplainCSharpCodeDirective : KernelActionDirective
{
    public ExplainCSharpCodeDirective() : base("#!explain")
    {
        Description = "Explain csharp code with Sequence diagrams.";
    }
}