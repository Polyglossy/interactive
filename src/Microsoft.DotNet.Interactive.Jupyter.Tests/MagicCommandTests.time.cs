// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using FluentAssertions;
using System.Threading.Tasks;
using FluentAssertions.Extensions;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.CSharp;
using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.Tests.Utility;
using Xunit;
using static Polyglossy.Interactive.Formatting.Tests.Tags;
using Polyglossy.Interactive.Formatting.Tests.Utility;

namespace Polyglossy.Interactive.Jupyter.Tests;

public partial class MagicCommandTests
{
    public class Time
    {
        [Fact]
        public async Task time_produces_time_elapsed_to_run_the_code_submission()
        {
            using var kernel = new CompositeKernel
                {
                    new CSharpKernel().UseKernelHelpers()
                }
                .UseDefaultMagicCommands();

            using var events = kernel.KernelEvents.ToSubscribedList();

            await kernel.SendAsync(new SubmitCode(
                @"
#!time

using System.Threading.Tasks;
await Task.Delay(500);
display(123);
"));

            events.Should()
                .ContainSingle<DisplayedValueProduced>(
                    e => e.As<DisplayedValueProduced>().Value is TimeSpan)
                .Which
                .FormattedValues
                .Should()
                .ContainSingle(v =>
                    v.MimeType == "text/plain" &&
                    v.Value.StartsWith("Wall time:") &&
                    v.Value.EndsWith("ms"));

            events.Should()
                .ContainSingle<DisplayedValueProduced>(
                    e => e.As<DisplayedValueProduced>().Value is int)
                .Which
                .FormattedValues
                .Should()
                .ContainSingle(v =>
                    v.MimeType == "text/html" &&
                    v.Value.RemoveStyleElement() == $"{PlainTextBegin}123{PlainTextEnd}");

            events.Should()
                .ContainSingle<DisplayedValueProduced>(
                    e => e.As<DisplayedValueProduced>().Value is TimeSpan)
                .Which
                .Value
                .As<TimeSpan>()
                .Should()
                .BeGreaterThanOrEqualTo(500.Milliseconds());
        }
    }
}