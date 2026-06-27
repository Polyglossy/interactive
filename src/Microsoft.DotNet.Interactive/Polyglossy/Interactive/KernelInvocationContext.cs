// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

using System;
using System.Globalization;
using System.Reflection;
using System.Threading;

namespace Polyglossy.Interactive;

public class KernelInvocationContext
{
    private readonly Microsoft.DotNet.Interactive.KernelInvocationContext _inner;

    public KernelInvocationContext(Polyglossy.Interactive.Commands.KernelCommand command)
    {
        _inner = (Microsoft.DotNet.Interactive.KernelInvocationContext)Activator.CreateInstance(
            typeof(Microsoft.DotNet.Interactive.KernelInvocationContext),
            BindingFlags.Instance | BindingFlags.NonPublic,
            binder: null,
            args: [command],
            culture: CultureInfo.InvariantCulture)!;
    }

    public Polyglossy.Interactive.Commands.KernelCommand Command => (Polyglossy.Interactive.Commands.KernelCommand)_inner.Command;

    public bool IsComplete => _inner.IsComplete;

    public CancellationToken CancellationToken => _inner.CancellationToken;

    public void Complete(Polyglossy.Interactive.Commands.KernelCommand command) => _inner.Complete(command);

    public void Fail(Polyglossy.Interactive.Commands.KernelCommand command, Exception? exception = null, string? message = null) => _inner.Fail(command, exception, message);
}
