using FluentAssertions;
using Microsoft.DotNet.Interactive.ExtensionLab.Inspector;
using Xunit;

namespace Microsoft.DotNet.Interactive.ExtensionLab.Tests;

public sealed class JitAsmDecompilerTests
{
    [Fact]
    public void jit_asm_output_contains_real_instructions()
    {
        var source = """
                     public static class JitAsmSample1
                     {
                         public static int Add(int a, int b) => a + b;
                     }
                     """;

        var result = Inspector.Inspector.Create(new InspectionOptions()).Compile(source);

        result.IsSuccess.Should().BeTrue();

        result.JitDecompilation
              .Should()
              .Contain("Add(")
              .And.MatchRegex(@"L0000: \S+")
              .And.NotContain("Failed to find HotColdRegions")
              .And.NotContain("Unknown Method");
    }

    [Fact]
    public void jit_asm_output_is_real_for_repeated_inspections_of_different_code()
    {
        // Methods that are JIT'd after an earlier inspection must still be visible in later ones.
        for (var i = 0; i < 3; i++)
        {
            var source = $$"""
                           public static class JitAsmSample2_{{i}}
                           {
                               public static int Multiply{{i}}(int a, int b) => a * b + {{i + 1}};
                           }
                           """;

            var result = Inspector.Inspector.Create(new InspectionOptions()).Compile(source);

            result.JitDecompilation
                  .Should()
                  .Contain($"Multiply{i}(")
                  .And.MatchRegex(@"L0000: \S+")
                  .And.NotContain("Failed to find HotColdRegions")
                  .And.NotContain("Unknown Method");
        }
    }
}
