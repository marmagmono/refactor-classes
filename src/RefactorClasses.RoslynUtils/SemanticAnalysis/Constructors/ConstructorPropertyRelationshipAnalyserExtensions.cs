using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

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
            var mappings = CreateArray<int>(properties.Length, NoMapping);
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

        private static T[] CreateArray<T>(int size, T value)
        {
            var res = new T[size];
            for (int i = 0; i < res.Length; ++i)
                res[i] = value;

            return res;
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
