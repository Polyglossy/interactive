// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.Formatting;

namespace Polyglossy.Interactive.Browser;

[TypeFormatterSource(typeof(BrowserDisplayEventFormatterSource))]
public class BrowserDisplayEvent
{
    public BrowserDisplayEvent(DisplayEvent displayEvent, int executionOrder)
    {
        DisplayEvent = displayEvent;
        ExecutionOrder = executionOrder;
    }

    public DisplayEvent DisplayEvent { get; }

    public int ExecutionOrder { get; }
}