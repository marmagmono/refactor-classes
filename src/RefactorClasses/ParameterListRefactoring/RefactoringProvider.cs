using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RefactorClasses.ParameterListRefactoring
{
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "ParameterListRefactroing"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, parameterList) = await context.FindSyntaxForCurrentSpan<ParameterListSyntax>();
            if (document == null || parameterList == null) return;

            var isMultiline = parameterList.OpenParenToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);
            if (isMultiline)
            {
                context.RegisterRefactoring(new DelegateCodeAction(
                    "Make parameters single line",
                    c => MakeSingleLine(document, parameterList)));
            }
            else
            {
                context.RegisterRefactoring(new DelegateCodeAction(
                    "Split parameters into multiple lines",
                    c => MakeMultiline(document, parameterList)));
            }
        }

        private static async Task<Document> MakeMultiline(Document document, ParameterListSyntax parameterList)
        {
            return document;
        }

        private static async Task<Document> MakeSingleLine(Document document, ParameterListSyntax parameterList)
        {
            return document;
        }
    }
}
