// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polyglossy.Interactive.Commands;

namespace Polyglossy.Interactive.SQLite;

public class ConnectSQLiteKernel : ConnectKernelCommand
{
    public ConnectSQLiteKernel(string connectedKernelName) : base(connectedKernelName)
    {
    }

    public string ConnectionString { get; set; }
}