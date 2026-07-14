using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Polyglossy.Interactive.Http.Parsing;
internal class HttpNamedRequestNameNode : HttpSyntaxNode
{
    public HttpNamedRequestNameNode(SourceText sourceText, HttpSyntaxTree syntaxTree) : base(sourceText, syntaxTree)
    {
    }
}
