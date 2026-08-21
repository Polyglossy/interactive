// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Polyglossy.Interactive.Directives;

namespace Polyglossy.Interactive.Jupyter;

public class SupportedDirectives
{
    public SupportedDirectives(string kernelName, IReadOnlyList<KernelDirective> directives)
    {
        KernelName = kernelName;
        Directives = directives;
    }

    public string KernelName { get; }

    public IReadOnlyList<KernelDirective> Directives { get; }
}