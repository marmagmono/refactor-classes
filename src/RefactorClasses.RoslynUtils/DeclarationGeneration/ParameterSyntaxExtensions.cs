using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    using SH = SyntaxHelpers;

    public static class ParameterSyntaxExtensions
    {
        public static ArgumentSyntax ToArgument(this ParameterSyntax ps) =>
            SH.ArgumentFromIdentifier(ps.Identifier);
    }
}
