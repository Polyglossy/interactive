// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Events;

namespace Polyglossy.Interactive.App.Events;

public class CodeExpansionInfosProduced : KernelEvent
{
    public CodeExpansionInfosProduced(IReadOnlyCollection<CodeExpansionInfo> codeExpansionInfos, KernelCommand command) : base(command)
    {
        CodeExpansionInfos = codeExpansionInfos ?? [];
    }

    public IReadOnlyCollection<CodeExpansionInfo> CodeExpansionInfos { get; }
}