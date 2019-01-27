using Microsoft.CodeAnalysis;
using System.Collections.Generic;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors
{
    public class ConstructorPropertyRelationshipAnalyserResult
    {
        private static readonly EmptyAssignmentAnalyserResult EmptyResult = new EmptyAssignmentAnalyserResult();

        private Dictionary<IFieldSymbol, AssignmentAnalyserResult> fieldResults;
        private Dictionary<IPropertySymbol, AssignmentAnalyserResult> propertyResults;

        public ConstructorPropertyRelationshipAnalyserResult(
            Dictionary<IFieldSymbol, AssignmentAnalyserResult> fieldResults,
            Dictionary<IPropertySymbol, AssignmentAnalyserResult> propertyResults)
        {
            this.fieldResults = fieldResults;
            this.propertyResults = propertyResults;
        }

        public AssignmentAnalyserResult GetResult(IFieldSymbol fieldSymbol)
        {
            if (fieldResults.TryGetValue(fieldSymbol, out var v))
            {
                return v;
            }

            return EmptyResult;
        }

        public AssignmentAnalyserResult GetResult(IPropertySymbol propertySymbol)
        {
            if (propertyResults.TryGetValue(propertySymbol, out var v))
            {
                return v;
            }

            return EmptyResult;
        }
    }
}
