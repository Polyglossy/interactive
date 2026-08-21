// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Text.Json.Serialization;
using Polyglossy.Interactive.Commands;

namespace Polyglossy.Interactive.ExtensionLab;

public class StartCommandTranscription : KernelCommand
{
    [JsonPropertyName("output")]
    public string OutputFile { get; set; }
}