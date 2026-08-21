// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using Polyglossy.Interactive.Jupyter.Messaging;
using Polyglossy.Interactive.Jupyter.Protocol;
using Message = Polyglossy.Interactive.Jupyter.Messaging.Message;

namespace Polyglossy.Interactive.Jupyter.ZMQ;

/// <summary>
/// jupyter has two request reply channels: "shell" and "control" on different sockets. 
/// </summary>
internal class RequestReplyChannel
{
    private readonly MessageSender _sender;
    private readonly string channel;

    public RequestReplyChannel(MessageSender sender, string channel = MessageChannelValues.shell)
    {
        _sender = sender ?? throw new ArgumentNullException(nameof(sender));
        this.channel = channel;
    }
    public void Reply(ReplyMessage message, Message request)
    {
        var reply = Message.CreateReply(message, request, channel);
        Send(reply);
    }

    public void Send(Message message)
    {
        _sender.Send(message);
    }
}