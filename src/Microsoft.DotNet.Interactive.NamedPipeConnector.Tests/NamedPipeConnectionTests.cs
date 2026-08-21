// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.IO.Pipes;
using System.Threading.Tasks;

using FluentAssertions;

using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Connection;
using Polyglossy.Interactive.CSharp;
using Polyglossy.Interactive.FSharp;
using Polyglossy.Interactive.Tests;

using Xunit;
using Xunit.Abstractions;

namespace Polyglossy.Interactive.NamedPipeConnector.Tests;

public class NamedPipeConnectionTests : ProxyKernelConnectionTestsBase
{
    private readonly string _pipeName = Guid.NewGuid().ToString();

    public NamedPipeConnectionTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public void connect_command_is_available_when_a_user_adds_a_kernel_connection_type()
    {
        using var compositeKernel = new CompositeKernel();

        compositeKernel.AddConnectDirective(new ConnectNamedPipeDirective());

        compositeKernel
            .KernelInfo
            .SupportedDirectives
            .Should()
            .Contain(c => c.Name == "#!connect");
    }

    protected override Func<string, Task<ProxyKernel>> CreateConnector()
    {
        CreateRemoteKernelTopology(_pipeName);

        var connector = new NamedPipeKernelConnector(_pipeName);

        RegisterForDisposal(connector);

        return connector.CreateKernelAsync;
    }

    protected override SubmitCode CreateSubmitCodeToConnectProxyAs(string localKernelName)
    {
        return new SubmitCode($"#!connect named-pipe --kernel-name {localKernelName} --pipe-name {_pipeName}");
    }

    protected override void AddConnectDirectiveTo(CompositeKernel compositeKernel)
    {
        CreateRemoteKernelTopology(_pipeName);

        compositeKernel.AddConnectDirective(new ConnectNamedPipeDirective());
    }
   
    private void CreateRemoteKernelTopology(string pipeName)
    {
        var remoteCompositeKernel = new CompositeKernel
        {
            new CSharpKernel(),
            new FSharpKernel()
        };

        remoteCompositeKernel.DefaultKernelName = "csharp";

        RegisterForDisposal(remoteCompositeKernel);

        var serverStream = new NamedPipeServerStream(
            pipeName,
            PipeDirection.InOut,
            1,
            PipeTransmissionMode.Message,
            PipeOptions.Asynchronous);

        var sender = KernelCommandAndEventSender.FromNamedPipe(
            serverStream,
            new Uri("kernel://remote"));

        var receiver = KernelCommandAndEventReceiver.FromNamedPipe(serverStream);

        var host = remoteCompositeKernel.UseHost(sender, receiver, new Uri("kernel://local"));

        var _ = Task.Run(() =>
        {
            // required as waiting connection on named pipe server will block
            serverStream.WaitForConnection();
            var _ = host.ConnectAsync();
        });

        RegisterForDisposal(host);
        RegisterForDisposal(receiver);
        RegisterForDisposal(serverStream);
    }
}