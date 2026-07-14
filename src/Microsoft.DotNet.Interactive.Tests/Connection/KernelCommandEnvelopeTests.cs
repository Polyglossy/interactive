// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using FluentAssertions;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Connection;
using Xunit;

namespace Polyglossy.Interactive.Tests.Connection;

public class KernelCommandEnvelopeTests
{
    [Fact]
    public void Create_creates_envelope_of_the_correct_type()
    {
        KernelCommand command = new SubmitCode("display(123)");

        var envelope = KernelCommandEnvelope.Create(command);

        envelope.Should().BeOfType<KernelCommandEnvelope<SubmitCode>>();
    }
        
    [Fact]
    public void Create_creates_envelope_with_reference_to_original_command()
    {
        KernelCommand command = new SubmitCode("display(123)");

        var envelope = KernelCommandEnvelope.Create(command);

        envelope.Command.Should().BeSameAs(command);
    }
}