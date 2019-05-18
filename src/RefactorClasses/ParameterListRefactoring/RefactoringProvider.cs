using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.ParameterListRefactoring
{
    using SF = SyntaxFactory;

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "ParameterListRefactroing"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, parameterList) = await context.FindSyntaxForCurrentSpan<ParameterListSyntax>().ConfigureAwait(false);
            if (document == null
                || parameterList == null
                || parameterList.Parameters.Count == 0)
                return;

            var isMultiline = parameterList.OpenParenToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);
            if (isMultiline)
            {
                context.RegisterRefactoring(new DelegateCodeAction(
                    "Make parameters single line",
                    c => MakeSingleLine(document, parameterList, c)));
            }
            else
            {
                context.RegisterRefactoring(new DelegateCodeAction(
                    "Split parameters into multiple lines",
                    c => MakeMultiline(document, parameterList, c)));
            }
        }

        private static async Task<Document> MakeMultiline(
            Document document,
            ParameterListSyntax parameterList,
            CancellationToken cancellationToken)
        {
            var parameters = parameterList.Parameters.ToList();
            if (parameters.Count == 0) return document;

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (tree == default) return document;

            var currentIndent = parameterList.EstimateIndent(tree, cancellationToken);
            var parameterIndent = currentIndent + Settings.IndentOneLevel;

            return await RewriteParameterList(
                document,
                parameterList,
                Settings.EndOfLine,
                Settings.EndOfLine,
                SF.Whitespace(parameterIndent),
                cancellationToken);
        }

        private static Task<Document> MakeSingleLine(
            Document document,
            ParameterListSyntax parameterList,
            CancellationToken cancellationToken)
        {
            return RewriteParameterList(
                document,
                parameterList,
                SF.Whitespace(string.Empty),
                SF.Whitespace(" "),
                SF.Whitespace(string.Empty),
                cancellationToken);
        }

        private static async Task<Document> RewriteParameterList(
            Document document,
            ParameterListSyntax parameterList,
            SyntaxTrivia openParenTrivia,
            SyntaxTrivia commaTrivia,
            SyntaxTrivia firstParameterTokenLeadingTrivia,
            CancellationToken cancellationToken)
        {
            var updatedParameters = parameterList.Parameters.Select(
                p => p.WithLeadingTrivia(firstParameterTokenLeadingTrivia));

            var separators = Enumerable.Repeat(
                SF.Token(SyntaxKind.CommaToken).WithTrailingTrivia(commaTrivia),
                parameterList.Parameters.Count() - 1);

            var updatedParameterList = SF.ParameterList(
                SF.Token(SyntaxKind.OpenParenToken).WithTrailingTrivia(openParenTrivia),
                SF.SeparatedList(updatedParameters, separators),
                parameterList.CloseParenToken);

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(parameterList, updatedParameterList);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
