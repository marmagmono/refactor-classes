using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationGeneration
{
    public static class ParameterListSyntaxExtensions
    {
        public static ArgumentSyntax[] ToArgumentArray(this ParameterListSyntax pls) =>
            pls.Parameters.Select(ParameterSyntaxExtensions.ToArgument).ToArray();
    }
}
