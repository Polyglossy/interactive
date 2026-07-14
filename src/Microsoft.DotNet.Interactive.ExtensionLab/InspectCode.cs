// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.CodeAnalysis;
using Polyglossy.Interactive.Commands;

namespace Polyglossy.Interactive.ExtensionLab;

public class InspectCode : KernelCommand
{
    public OptimizationLevel Configuration { get; set; } = OptimizationLevel.Debug;

    public SourceCodeKind Kind { get; set; } = SourceCodeKind.Script;

    public Platform Platform { get; set; }
}