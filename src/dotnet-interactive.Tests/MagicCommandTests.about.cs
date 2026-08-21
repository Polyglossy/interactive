// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Threading.Tasks;
using FluentAssertions;
using Polyglossy.Interactive.Commands;
using Polyglossy.Interactive.Events;
using Polyglossy.Interactive.Tests.Utility;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Polyglossy.Interactive.App.Tests;

public class MagicCommandTests
{
    [Collection("Do not parallelize")]
    public class About
    {
        [Fact]
        public async Task it_shows_the_product_name_and_version_information()
        {
            using var kernel = new CompositeKernel()
                .UseAboutMagicCommand();

            var result = await kernel.SendAsync(new SubmitCode("#!about"));

            result.Events.Should()
                  .ContainSingle<DisplayedValueProduced>()
                  .Which
                  .FormattedValues
                  .Should()
                  .ContainSingle(v => v.MimeType == "text/html")
                  .Which
                  .Value
                  .As<string>()
                  .Should()
                  .ContainAll(
                      ".NET Interactive",
                      "Version",
                      "Library version",
                      "https://github.com/dotnet/interactive");
        }
    }
}