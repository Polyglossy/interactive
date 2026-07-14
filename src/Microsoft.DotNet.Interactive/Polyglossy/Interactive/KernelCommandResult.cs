// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace Microsoft.DotNet.Interactive;

public class KernelCommandResult : Polyglossy.Interactive.KernelCommandResult
{
    public KernelCommandResult(Polyglossy.Interactive.Commands.KernelCommand command)
        : base(command)
    {
    }
}
