using Microsoft.CodeAnalysis;

namespace RefactorClasses.GenerateWithFromProperties
{
    internal static class WithRefactoringUtils
    {
        public static string MethodName(SyntaxToken identifier) => $"With{identifier.WithoutTrivia().ValueText}";
    }
}
