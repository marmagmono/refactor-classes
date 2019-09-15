using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Class.Semantic
{
    public class FieldInfo
    {
        public FieldInfo(IFieldSymbol symbol, VariableDeclaratorSyntax variable)
        {
            Symbol = symbol;
            Variable = variable;
        }

        public IFieldSymbol Symbol { get; }

        public VariableDeclaratorSyntax Variable { get; }
    }
}
