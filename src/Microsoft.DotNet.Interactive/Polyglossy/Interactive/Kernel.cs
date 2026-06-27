// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace Polyglossy.Interactive;

public abstract class Kernel : Microsoft.DotNet.Interactive.Kernel
{
    protected Kernel(string name)
        : base(name)
    {
    }
}
