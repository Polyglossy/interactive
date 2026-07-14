// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace Microsoft.DotNet.Interactive.Events;

public abstract class KernelEvent : Polyglossy.Interactive.Events.KernelEvent
{
    protected KernelEvent(Polyglossy.Interactive.Commands.KernelCommand command)
        : base(command)
    {
    }
}
