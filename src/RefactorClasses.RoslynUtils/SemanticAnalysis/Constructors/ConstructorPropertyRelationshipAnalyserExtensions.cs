using Microsoft.CodeAnalysis;
using RefactorClasses.RoslynUtils.Utils;
using System;
using System.Collections.Immutable;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Constructors
{
    public static class ConstructorPropertyRelationshipAnalyserExtensions
    {
        public const int NoMapping = -1;

        public static (int[] indexMappings, bool isFullMatch) GetIndexMapping(
            this ConstructorPropertyRelationshipAnalyserResult relationships,
            IPropertySymbol[] properties,
            IMethodSymbol constructorSymbol)
        {
            if (properties == null || properties.Length == 0 || constructorSymbol == null)
                return (Array.Empty<int>(), false);

            var parameters = constructorSymbol.Parameters;
            var mappings = ArrayUtils.CreateArray(properties.Length, NoMapping);
            bool allPropertiesMatchParameters = properties.Length == parameters.Length;

            for (int i = 0; i < properties.Length; ++i)
            {
                mappings[i] = GetMatchingParameterIdx(
                    relationships.GetResult(properties[i]),
                    parameters);

                if (mappings[i] == NoMapping)
                    allPropertiesMatchParameters = false;
            }

            return (mappings, allPropertiesMatchParameters);
        }

        public static int GetMatchingParameterIdx(
            this ConstructorPropertyRelationshipAnalyserResult relationships,
            IMethodSymbol constructorSymbol,
            ISymbol symbol)
        {
            if (symbol == null) return -1;

            return GetMatchingParameterIdx(relationships.GetResult(symbol), constructorSymbol.Parameters);
        }

        private static int GetMatchingParameterIdx(
            AssignmentAnalyserResult analyserResult,
            ImmutableArray<IParameterSymbol> parameters)
        {
            switch (analyserResult)
            {
                case AssignmentExpressionAnalyserResult match:
                    return parameters.IndexOf(match.AssignedParameter);

                default:
                    return NoMapping;
            }
        }
    }
}
