// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Polyglossy.Interactive.Jupyter.Messaging;
using System;
using System.Threading.Tasks;

namespace Polyglossy.Interactive.Jupyter.Connection;

public interface IJupyterKernelConnection : IDisposable
{
    Uri Uri { get; }

    Task StartAsync();

    IMessageSender Sender { get; }

    IMessageReceiver Receiver { get; }
}
