// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable
namespace Polyglossy.Interactive.Directives;

public abstract partial class KernelDirective
{
    internal ParseKernelCommandDelegate? TryGetKernelCommandAsync { get; set; }
}