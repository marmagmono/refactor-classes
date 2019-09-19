using System;
using System.Collections.Generic;
using System.Linq;
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

        public TypeSyntax Type => this.syntax.Type;

        //public string GetTypeName
        //{
        //    get
        //    {
        //        // TODO: handle things like tuples, arrays
        //        switch (Syntax.Type)
        //        {
        //            case PredefinedTypeSyntax predefinedType:
        //                return predefinedType.Keyword.ValueText;
        //            case IdentifierNameSyntax identifierName:
        //                return identifierName.Identifier.ValueText;
        //            default:
        //                return null;
        //        }
        //    }
        //}

        public bool IsIn() => this.syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InKeyword));

        public bool IsOut() => this.syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.OutKeyword));

        public bool IsRef() => this.syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.RefKeyword));
    }
}
