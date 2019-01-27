using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Text;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors
{
    public abstract class AssignmentAnalyserResult { }

    public sealed class EmptyAssignmentAnalyserResult : AssignmentAnalyserResult { }

    /// <summary>
    /// <see cref="MultipleAssignments"/> represents a case in which property is assigned multiple times
    /// or in an if expression.
    /// </summary>
    public sealed class MultipleAssignments : AssignmentAnalyserResult { }

    /// <summary>
    /// <see cref="ParsingError"/> means that the assignment expression was to complicated for current parser.
    /// </summary>
    public sealed class ParsingError : AssignmentAnalyserResult { }

    /// <summary>
    /// <see cref="AssignmentExpressionAnalyserResult"/> contains information about
    /// a found relationship between a class member and constructor parameter.
    /// </summary>
    public sealed class AssignmentExpressionAnalyserResult : AssignmentAnalyserResult
    {
        public AssignmentExpressionAnalyserResult(
            AssignmentExpressionSyntax assignment,
            IParameterSymbol assignedParameter)
        {
            Assignment = assignment;
            AssignedParameter = assignedParameter;
        }

        /// <summary>
        /// Expression which assigns to a result.
        /// </summary>
        public AssignmentExpressionSyntax Assignment { get; }

        /// <summary>
        /// Constructor parameter that is assigned to field.
        /// </summary>
        public IParameterSymbol AssignedParameter { get; }
    }
}
