using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Parameter
{
    public class ParameterInspector
    {
        private readonly ParameterSyntax syntax;

        public ParameterInspector(ParameterSyntax syntax)
        {
            this.syntax = syntax;
        }

        public ParameterSyntax Syntax => this.syntax;

        public string Name => this.syntax.Identifier.WithoutTrivia().ValueText;

        public bool IsIn() => this.syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InKeyword));

        public bool IsOut() => this.syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword));

        public bool IsRef() => this.syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword));
    }
}
