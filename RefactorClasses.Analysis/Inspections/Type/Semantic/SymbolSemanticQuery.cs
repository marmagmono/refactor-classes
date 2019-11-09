using System.Linq;
using Microsoft.CodeAnalysis;

namespace RefactorClasses.Analysis.Inspections.Type.Semantic
{
    public static class SymbolSemanticQuery
    {
        public static IsTaskResult IsTask(ISymbol symbol)
        {
            var namedSymbol = symbol as INamedTypeSymbol;
            if (namedSymbol == null)
            {
                return IsTaskResult.NotATask();
            }

            if (namedSymbol.Name == "Task"
                && namedSymbol?.ContainingNamespace?.ToString() == "System.Threading.Tasks")
            {
                var firstTypeArg = namedSymbol.TypeArguments.FirstOrDefault();
                if (firstTypeArg != null)
                {
                    return IsTaskResult.TypedTask(firstTypeArg);
                }
                else
                {
                    return IsTaskResult.Task();
                }
            }

            return IsTaskResult.NotATask();
        }

        public static string GetName(ITypeSymbol symbol) =>
            symbol.OriginalDefinition != null ?
                symbol.OriginalDefinition.Name
                : symbol.Name;
    }

    public readonly struct IsTaskResult
    {
        public static IsTaskResult NotATask() => new IsTaskResult(ResultType.NotATask);

        public static IsTaskResult Task() => new IsTaskResult(ResultType.Task);

        public static IsTaskResult TypedTask(ITypeSymbol taskType) => new IsTaskResult(
            ResultType.TypedTask, taskType);

        public bool IsNotATask() => this.resultType == ResultType.NotATask;

        public bool IsTask() => this.resultType == ResultType.Task;

        public bool IsTypedTask(out ITypeSymbol taskType)
        {
            if (this.resultType == ResultType.TypedTask)
            {
                taskType = this.typeSymbol;
                return true;
            }
            else
            {
                taskType = null;
                return false;
            }
        }

        #region private
        private enum ResultType { NotATask, Task, TypedTask }

        private readonly ResultType resultType;

        private readonly ITypeSymbol typeSymbol;

        private IsTaskResult(ResultType resultType, ITypeSymbol typeSymbol = default)
        {
            this.resultType = resultType;
            this.typeSymbol = typeSymbol;
        }
        #endregion
    }
}
