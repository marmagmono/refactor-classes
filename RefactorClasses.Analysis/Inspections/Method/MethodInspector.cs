using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.Inspections.Flow;
using RefactorClasses.Analysis.Inspections.Method;
using RefactorClasses.Analysis.Inspections.Method.Semantic;
using RefactorClasses.Analysis.Inspections.Parameter;

namespace RefactorClasses.Analysis.Inspections.Method
{
    public class MethodInspector
    {
        // TODO: method vs constructor vs destructor vs operator vs conversion operator
        private readonly BaseMethodDeclarationSyntax syntax;

        public MethodInspector(BaseMethodDeclarationSyntax syntax)
        {
            this.syntax = syntax;
        }

        public static MethodInspector Create(BaseMethodDeclarationSyntax syntax) => new MethodInspector(syntax);

        public TestFlow Check(Func<BaseMethodQuery, bool> test)
        {
            var query = new BaseMethodQuery(this.syntax);
            return new TestFlow(test(query));
        }

        public TestFlow CheckParameters(Func<IEnumerable<ParameterInspector>, bool> test)
        {
            var pInspectors = this.syntax.ParameterList.Parameters.Select(
                p => new ParameterInspector(p));

            return new TestFlow(test(pInspectors));
        }

        public IEnumerable<ParameterInspector> Parameters =>
            this.syntax.ParameterList.Parameters.Select(p => new ParameterInspector(p));

        public BaseMethodDeclarationSyntax Syntax => this.syntax;

        public MethodSemanticQuery CreateSemanticQuery(SemanticModel semanticModel) =>
            new MethodSemanticQuery(semanticModel, this);

        public string Name
        {
            get
            {
                switch (syntax)
                {
                    case MethodDeclarationSyntax ms:
                        return ms.Identifier.WithoutTrivia().ValueText;
                    default:
                        return string.Empty;
                }
            }
        }

        // FindParameter
        // Analyze body -> arrow / block
        // analyze return type ?
        // generics
    }
}
