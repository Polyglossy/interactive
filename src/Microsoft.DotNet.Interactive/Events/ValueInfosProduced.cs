// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.ValueSharing;

namespace Polyglossy.Interactive.Events;

public class ValueInfosProduced : KernelEvent
{
    public ValueInfosProduced(IReadOnlyCollection<KernelValueInfo> valueInfos, RequestValueInfos command) : base(command)
    {
        ValueInfos = valueInfos ?? [];
    }

    public IReadOnlyCollection<KernelValueInfo> ValueInfos { get; }
}