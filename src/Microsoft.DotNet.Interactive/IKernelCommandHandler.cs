// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using Polyglossy.Interactive.Commands;

namespace Polyglossy.Interactive;

public interface IKernelCommandHandler<in TCommand> where TCommand: KernelCommand
{
    Task HandleAsync(TCommand command, KernelInvocationContext context);
}