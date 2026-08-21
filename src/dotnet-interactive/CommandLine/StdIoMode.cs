// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;

namespace Polyglossy.Interactive.App.CommandLine;

internal static class StdIoMode
{
    public static async Task<int> Do(StartupOptions startupOptions, KernelHost kernelHost)
    {
        await kernelHost.ConnectAndWaitAsync();
        return 0;
    }
}