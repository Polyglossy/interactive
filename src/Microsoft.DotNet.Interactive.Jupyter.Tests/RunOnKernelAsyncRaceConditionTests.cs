using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.DotNet.Interactive.Jupyter.Connection;
using Microsoft.DotNet.Interactive.Jupyter.Messaging;
using Microsoft.DotNet.Interactive.Jupyter.Protocol;
using Xunit;
using Xunit.Abstractions;
using Message = Microsoft.DotNet.Interactive.Jupyter.Messaging.Message;

namespace Microsoft.DotNet.Interactive.Jupyter.Tests;

/// <summary>
/// Regression test for the hot-observable race condition in JupyterKernel.RunOnKernelAsync.
///
/// Root cause: on fast Linux GHA runners, an in-process Jupyter kernel can complete and
/// publish its reply synchronously inside IMessageSender.SendAsync — before the caller
/// has a chance to subscribe via reply.ToTask(). Because IMessageReceiver.Messages is a
/// hot observable (no replay buffer), the reply is permanently missed and the task hangs.
///
/// Fix: subscribe (ToTask) BEFORE calling SendAsync, so the subscription is active when
/// the reply fires regardless of whether it arrives synchronously or asynchronously.
///
/// This test exercises the full production stack:
///   ConnectJupyterKernelDirective -> JupyterKernelConnector.CreateKernelAsync
///   -> JupyterKernel.CreateAsync -> RunOnKernelAsync (the race-critical path)
/// </summary>
[Collection("Do not parallelize")]
public class RaceConditionRegressionTests : JupyterKernelTestBase
{
    public RaceConditionRegressionTests(ITestOutputHelper output) : base(output) { }

    [Fact(Timeout = 10_000)]
    public async Task CreateKernelAsync_does_not_hang_when_KernelInfoReply_arrives_synchronously()
    {
        // Arrange: a tracker whose SendAsync fires KernelInfoReply synchronously before
        // returning — exactly what happens on fast Linux GHA runners with in-process kernels.
        using var tracker = new SynchronousKernelInfoTracker();
        var kernelConnection = new TestJupyterKernelConnection(tracker);
        var jupyterConnection = new TestJupyterConnection(kernelConnection);
        var options = new SimulatedJupyterConnectionOptions(jupyterConnection);

        // Act: exercises JupyterKernel.CreateAsync -> RunOnKernelAsync.
        // With the fix (subscribe before send) this completes well within the timeout.
        // With the upstream code (subscribe after send) the reply is missed and this hangs.
        var kernel = await CreateJupyterKernelAsync(options);

        // Assert
        kernel.Should().NotBeNull();
    }

    /// <summary>
    /// An IMessageTracker that fires KernelInfoReply synchronously inside SendAsync,
    /// simulating a fast in-process kernel that completes on the same call stack.
    /// Language name "testlang" is intentionally unknown so CommCommandEventChannelConfiguration
    /// skips its additional setup (no extra messages needed from this tracker).
    /// </summary>
    private sealed class SynchronousKernelInfoTracker : IMessageTracker
    {
        private readonly Subject<Message> _sent = new();
        private readonly Subject<Message> _received = new();

        public IObservable<Message> Messages => _received;
        public IObservable<Message> SentMessages => _sent;
        public IObservable<Message> ReceivedMessages => _received;

        public Task SendAsync(Message message)
        {
            _sent.OnNext(message);
            var reply = Message.CreateReply(
                new KernelInfoReply("5.3", "test", "1.0",
                    new LanguageInfo("testlang", "1.0", "text/plain", ".txt")),
                message);
            // Fire synchronously BEFORE returning — this is the race trigger.
            _received.OnNext(reply);
            return Task.CompletedTask;
        }

        public void Attach(IMessageSender sender, IMessageReceiver receiver) { }

        public void Dispose()
        {
            _sent.Dispose();
            _received.Dispose();
        }
    }
}
