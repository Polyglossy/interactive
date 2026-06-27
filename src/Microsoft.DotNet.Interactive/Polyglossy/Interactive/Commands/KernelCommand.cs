// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace Polyglossy.Interactive.Commands;

public abstract class KernelCommand : Microsoft.DotNet.Interactive.Commands.KernelCommand
{
    protected KernelCommand(string? targetKernelName = null)
        : base(targetKernelName)
    {
    }
}
