// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.Tests;
using Polyglossy.Interactive.Tests.Utility;
using Xunit;
using Xunit.Abstractions;

namespace Polyglossy.Interactive.CSharp.Tests;

public class CSharpKernelTests : LanguageKernelTestBase
{
    public CSharpKernelTests(ITestOutputHelper output) : base(output)
    {
    }

    [Fact]
    public async Task Script_state_is_available_within_middleware_pipeline()
    {
        var variableCountBeforeEvaluation = 0;
        var variableCountAfterEvaluation = 0;

        using var kernel = new CSharpKernel();

        kernel.AddMiddleware(async (command, context, next) =>
        {
            var k = context.HandlingKernel as CSharpKernel;

            await next(command, context);

            variableCountAfterEvaluation = k.ScriptState.Variables.Length;
        });

        await kernel.SendAsync(new SubmitCode("var x = 1;"));

        variableCountBeforeEvaluation.Should().Be(0);
        variableCountAfterEvaluation.Should().Be(1);
    }

    [Fact]
    public async Task GetValueInfos_only_returns_non_shadowed_values()
    {
        using var kernel = new CSharpKernel();

        await kernel.SendAsync(new SubmitCode("var x = 1;"));
        await kernel.SendAsync(new SubmitCode("var x = \"two\";"));

        var (success, valueInfosProduced) = await kernel.TryRequestValueInfosAsync();

        success.Should().BeTrue();

        valueInfosProduced.ValueInfos
            .Should()
            .ContainSingle(v => v.Name == "x");
    }

    [Fact]
    public async Task Use_of_interactive_API_in_submitted_code_does_not_produce_diagnostics()
    {
        using var kernel = new CSharpKernel();

        var result1 = await kernel.SendAsync(
                          new SubmitCode(
                              """
                              using Polyglossy.Interactive;
                              using Polyglossy.Interactive.Commands;
                              using Polyglossy.Interactive.Events;

                              Kernel.Root.GetType()
                              """));

        result1.Events.Should().NotContainErrors();
        result1.Events.OfType<DiagnosticsProduced>()
               .SelectMany(d => d.FormattedDiagnostics)
               .Should().BeEmpty();

        var result2 = await kernel.SendAsync(
                          new RequestDiagnostics(
                              """
                              using Polyglossy.Interactive;
                              using Polyglossy.Interactive.Commands;
                              using Polyglossy.Interactive.Events;

                              
                              
                              Kernel.Root.GetType()
                              """));

        result2.Events.Should().NotContainErrors();
        result2.Events.OfType<DiagnosticsProduced>()
               .SelectMany(d => d.FormattedDiagnostics)
               .Should().BeEmpty();
    }
}