// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Linq;
using FluentAssertions;
using Polyglossy.Interactive.Http.Parsing;
using Polyglossy.Interactive.Parsing.Tests.Utility;
using Xunit;

namespace Polyglossy.Interactive.Http.Tests;

public partial class HttpParserTests
{
    public class Trivia
    {
        [Fact]
        public void it_can_parse_an_empty_string()
        {
            var result = Parse("");
            result.SyntaxTree.Should().NotBeNull();
        }

        [Fact]
        public void it_can_parse_a_string_with_only_whitespace()
        {
            var result = Parse(" \t ");

            result.SyntaxTree.RootNode
                  .ChildTokens.First().Text.Should().Be(" \t ");
        }

        [Fact]
        public void string_with_only_newlines_is_parsed_into_root_node()
        {
            var result = Parse("\r\n\n\r\n");

            result.SyntaxTree.RootNode
                  .FullText.Should().Be("\r\n\n\r\n");
        }
    }
}