// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Diagnostics;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Formatting;

namespace Polyglossy.Interactive.Events;

[DebuggerStepThrough]
public class DisplayedValueProduced : DisplayEvent
{
    public DisplayedValueProduced(
        object value,
        KernelCommand command,
        IReadOnlyCollection<FormattedValue> formattedValues = null,
        string valueId = null) : base(value, command, formattedValues, valueId)
    {
    }
}