// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#nullable enable

namespace Microsoft.DotNet.Interactive;

public class KernelInfo : Polyglossy.Interactive.KernelInfo
{
    public KernelInfo(
        string localName,
        string[]? aliases = null,
        bool isProxy = false,
        bool isComposite = false,
        string? description = null)
        : base(localName, aliases, isProxy, isComposite, description)
    {
    }
}
