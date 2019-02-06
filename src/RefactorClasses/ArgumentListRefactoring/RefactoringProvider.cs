using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using RefactorClasses.CodeActions;
using RefactorClasses.CodeRefactoringUtils;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using RefactorClasses.RoslynUtils.DeclarationGeneration;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.ArgumentListRefactoring
{
    using SF = SyntaxFactory;
    using SH = SyntaxHelpers;

    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = "ParameterListRefactroing"), Shared]
    public class RefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            var (document, argumentList) = await context.FindSyntaxForCurrentSpan<ArgumentListSyntax>().ConfigureAwait(false);
            if (document == null
                || argumentList == null
                || argumentList.Arguments.Count == 0)
                return;

            var isMultiline = argumentList.OpenParenToken.TrailingTrivia.Any(SyntaxKind.EndOfLineTrivia);
            if (isMultiline)
            {
                context.RegisterRefactoring(new DelegateCodeAction(
                    "Make arguments single line",
                    c => MakeSingleLine(document, argumentList, c)));
            }
            else
            {
                context.RegisterRefactoring(new DelegateCodeAction(
                    "Split arguments into multiple lines",
                    c => MakeMultiline(document, argumentList, c)));
            }
        }

        private static async Task<Document> MakeMultiline(
            Document document,
            ArgumentListSyntax argumentList,
            CancellationToken cancellationToken)
        {
            var arguments = argumentList.Arguments.ToList();
            if (arguments.Count == 0) return document;

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            if (tree == default) return document;

            var currentIndent = argumentList.EstimateIndent(tree, cancellationToken);
            var indent = currentIndent + Settings.IndentOneLevel;

            return await RewriteArgumentList(
                document,
                argumentList,
                Settings.EndOfLine,
                Settings.EndOfLine,
                SF.Whitespace(indent),
                cancellationToken);
        }

        private static Task<Document> MakeSingleLine(
            Document document,
            ArgumentListSyntax argumentList,
            CancellationToken cancellationToken)
        {
            return RewriteArgumentList(
                document,
                argumentList,
                SF.Whitespace(string.Empty),
                SF.Whitespace(" "),
                SF.Whitespace(string.Empty),
                cancellationToken);
        }

        private static async Task<Document> RewriteArgumentList(
            Document document,
            ArgumentListSyntax argumentList,
            SyntaxTrivia openParenTrivia,
            SyntaxTrivia commaTrivia,
            SyntaxTrivia parameterTypeTrivia,
            CancellationToken cancellationToken)
        {
            var updatedArguments = argumentList.Arguments.Select(
                p => p.WithLeadingTrivia(parameterTypeTrivia));

            var separators = Enumerable.Repeat(
                SF.Token(SyntaxKind.CommaToken).WithTrailingTrivia(commaTrivia),
                argumentList.Arguments.Count() - 1);

            var updatedParameterList = SF.ArgumentList(
                SF.Token(SyntaxKind.OpenParenToken).WithTrailingTrivia(openParenTrivia),
                SF.SeparatedList(updatedArguments, separators),
                argumentList.CloseParenToken);

            var tree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);
            var root = await tree.GetRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root.ReplaceNode(argumentList, updatedParameterList);
            var newDocument = document.WithSyntaxRoot(newRoot);
            return newDocument;
        }
    }
}
