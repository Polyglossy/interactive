// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polyglossy.Interactive.Commands;
using System.Collections.Generic;

namespace Polyglossy.Interactive.Journey;

public class LessonDefinition
{
    public string Name { get; }
    public IReadOnlyList<SubmitCode> Setup { get; }

    public LessonDefinition(string name, IReadOnlyList<SubmitCode> setup)
    {
        Name = name;
        Setup = setup;
    }
}