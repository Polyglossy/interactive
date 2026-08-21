// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Polyglossy.Interactive.Jupyter.Protocol;
using Message = Polyglossy.Interactive.Jupyter.Messaging.Message;

namespace Polyglossy.Interactive.Jupyter.ZMQ;

internal class PubSubChannel 
{
    private readonly MessageSender _sender;

    public PubSubChannel(MessageSender sender)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
    }
    public void Publish(PubSubMessage message, Message request, string kernelIdentity)
    {
        var reply = Message.CreatePubSub(message, request, kernelIdentity);
        _sender.Send(reply);
    }
}