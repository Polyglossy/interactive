// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.Formatting;

namespace Polyglossy.Interactive;

[TypeFormatterSource(typeof(MessageDiagnosticsFormatterSource))]
public class KernelCommandResult
{
    private readonly List<KernelEvent> _events = new();

    internal KernelCommandResult(KernelCommand command)
    {
        Command = command;
    }

    public KernelCommand Command { get; }

    public IReadOnlyList<KernelEvent> Events => _events;

    internal void AddEvent(KernelEvent @event)
    {
        if (!@event.Command.Equals(Command))
        {
            if (!Command.ShouldResultIncludeEventsFrom(@event.Command))
            {
                return;
            }
        }

        _events.Add(@event);
    }
}