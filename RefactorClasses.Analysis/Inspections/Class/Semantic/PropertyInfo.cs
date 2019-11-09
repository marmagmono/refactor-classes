using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Class.Semantic
{
    public class PropertyInfo
    {
        public PropertyInfo(IPropertySymbol symbol, PropertyDeclarationSyntax declaration)
        {
            Symbol = symbol;
            Declaration = declaration;
        }

        public IPropertySymbol Symbol { get; }

        public PropertyDeclarationSyntax Declaration { get; }
    }
}
