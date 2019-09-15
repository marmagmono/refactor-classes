using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;

namespace RefactorClasses.Analysis.Inspections.Method.Semantic
{
    public class MethodSemanticQuery
    {
        private readonly SemanticModel model;
        private readonly MethodInspector inspector;

        public MethodSemanticQuery(SemanticModel model, MethodInspector inspector)
        {
            this.model = model;
            this.inspector = inspector;
        }

        public AssignmentsResult FindAssignments()
        {
            var parameterSymbols =
                this.inspector.Parameters
                    .Select(ps => (IParameterSymbol)model.GetDeclaredSymbol(ps))
                    .ToList();

            var finder = new ParametersAssignmentsFinder(parameterSymbols, this.model);
            finder.Visit(this.inspector.Syntax);
            return finder.GetAssignedParameters();
        }
    }
}
