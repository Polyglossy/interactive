using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.DotNet.Interactive.Commands;
using Microsoft.DotNet.Interactive.Events;
using Microsoft.DotNet.Interactive.Tests.Utility;
using Xunit;

namespace Microsoft.DotNet.Interactive.CSharp.Tests;

// These tests deliberately avoid C# 15 features that need runtime support newer than net10.0 (for example `union` types).
public class CSharp15Tests
{
    [Fact]
    public void Kernel_reports_C_sharp_15_as_its_language_version()
    {
        using var kernel = new CSharpKernel();

        var expected = LanguageVersion.Latest.MapSpecifiedToEffectiveVersion().ToDisplayString();

        kernel.KernelInfo.LanguageVersion.Should().Be(expected);
        kernel.KernelInfo.LanguageVersion.Should().Be("15.0");
    }

    [Fact]
    public void LanguageVersion_Latest_resolves_to_C_sharp_15()
    {
        LanguageVersion.Latest.MapSpecifiedToEffectiveVersion().Should().Be(LanguageVersion.CSharp15);
    }

    [Fact]
    public async Task Labeled_continue_and_break_are_accepted_and_run()
    {
        using var kernel = new CSharpKernel();

        var result = await kernel.SendAsync(
                         new SubmitCode(
                             """
                             var visited = new System.Collections.Generic.List<string>();

                             outer: for (int row = 0; row < 3; row++)
                             {
                                 for (int column = 0; column < 3; column++)
                                 {
                                     if (column == 1)
                                     {
                                         continue outer;
                                     }

                                     if (row == 2)
                                     {
                                         break outer;
                                     }

                                     visited.Add($"{row},{column}");
                                 }
                             }

                             string.Join(";", visited)
                             """));

        result.Events.Should().NotContainErrors();

        result.Events
              .Should()
              .ContainSingle<ReturnValueProduced>()
              .Which
              .Value
              .Should()
              .Be("0,0;1,0");
    }

    [Fact]
    public async Task Collection_expression_arguments_are_accepted_and_run()
    {
        using var kernel = new CSharpKernel();

        var result = await kernel.SendAsync(
                         new SubmitCode(
                             """
                             string[] values = ["Hello", "HELLO", "hello"];

                             System.Collections.Generic.HashSet<string> set = [with(System.StringComparer.OrdinalIgnoreCase), .. values];

                             set.Count
                             """));

        result.Events.Should().NotContainErrors();

        result.Events
              .Should()
              .ContainSingle<ReturnValueProduced>()
              .Which
              .Value
              .Should()
              .Be(1);
    }

    [Fact]
    public async Task RequestDiagnostics_does_not_report_errors_for_C_sharp_15_syntax()
    {
        using var kernel = new CSharpKernel();

        var result = await kernel.SendAsync(
                         new RequestDiagnostics(
                             """
                             outer: for (int i = 0; i < 2; i++)
                             {
                                 for (int j = 0; j < 2; j++)
                                 {
                                     if (j == 0)
                                     {
                                         continue outer;
                                     }
                                 }
                             }
                             """));

        // No DiagnosticsProduced event is emitted when there is nothing to report, so also check that the command succeeded.
        result.Events.Should().ContainSingle<CommandSucceeded>();

        result.Events
              .OfType<DiagnosticsProduced>()
              .SelectMany(d => d.Diagnostics)
              .Should()
              .BeEmpty();

        // Control: the same request does produce a diagnostic when the label doesn't exist.
        var errorResult = await kernel.SendAsync(
                              new RequestDiagnostics(
                                  """
                                  for (int i = 0; i < 2; i++)
                                  {
                                      continue missing;
                                  }
                                  """));

        errorResult.Events
                   .Should()
                   .ContainSingle<DiagnosticsProduced>()
                   .Which
                   .Diagnostics
                   .Should()
                   .ContainSingle(d => d.Severity == DiagnosticSeverity.Error);
    }
}
