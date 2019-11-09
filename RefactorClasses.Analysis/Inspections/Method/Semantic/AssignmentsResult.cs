using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Method.Semantic
{
    public class Assignment
    {
        public int ParameterIdx { get; }

        public IParameterSymbol Parameter { get; }

        public ISymbol AssignedSymbol { get; }

        public AssignmentExpressionSyntax AssignmentExpression { get; }

        public Assignment(int parameterIdx, IParameterSymbol parameter, ISymbol assignedSymbol, AssignmentExpressionSyntax assignmentExpression)
        {
            ParameterIdx = parameterIdx;
            Parameter = parameter;
            AssignedSymbol = assignedSymbol;
            AssignmentExpression = assignmentExpression;
        }
    }

    public class NotAssignedParameter
    {
        public int ParameterIdx { get; }

        public IParameterSymbol Parameter { get; }

        public NotAssignedParameter(int parameterIdx, IParameterSymbol parameter)
        {
            ParameterIdx = parameterIdx;
            Parameter = parameter;
        }
    }

    public class AssignmentsResult
    {
        public IReadOnlyList<Assignment> Assignments { get; }

        public IReadOnlyList<NotAssignedParameter> NotAssignedParameters { get; }

        public AssignmentsResult(IReadOnlyList<Assignment> assignments, IReadOnlyList<NotAssignedParameter> notAssignedParameters)
        {
            Assignments = assignments;
            NotAssignedParameters = notAssignedParameters;
        }
    }
}
