// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Polyglossy.Interactive.CSharpProject;

public class Project
{
    public IReadOnlyList<ProjectFile> Files { get; }

    public Project(IReadOnlyList<ProjectFile> files)
    {
        Files = files;
    }
}
