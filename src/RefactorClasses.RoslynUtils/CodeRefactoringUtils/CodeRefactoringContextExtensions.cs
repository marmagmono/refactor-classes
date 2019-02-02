using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Threading.Tasks;

namespace RefactorClasses.CodeRefactoringUtils
{
    public static class CodeRefactoringContextExtensions
    {
        public static async Task<(SyntaxNode root, SyntaxToken token)>
            GetSyntaxContext(this CodeRefactoringContext context)
        {
            var document = context.Document;
            var textSpan = context.Span;
            var cancellationToken = context.CancellationToken;

            if (!textSpan.IsEmpty) return (null, default(SyntaxToken));

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var token = root.FindToken(textSpan.Start);

            return (root, token);
        }

        public static async Task<(Document doc, TNode syntaxElement)>
            FindSyntaxForCurrentSpan<TNode>(this CodeRefactoringContext context) where TNode : SyntaxNode
        {
            var document = context.Document;

            var (root, token) = await GetSyntaxContext(context);
            if (token.Parent == null) return (null, null);

            var syntax = token.Parent.FirstAncestorOrSelf<TNode>();
            if (syntax == null) return (null, null);

            return (document, syntax);
        }

        public static async Task<(Document doc, VariableDeclaratorSyntax)>
            FindVariableDeclaratorForCurrentSpan(this CodeRefactoringContext context)
        {
            // Verify if it is not a comma or semicolon token.
            var document = context.Document;
            var span = context.Span;
            if (span.Start == 0) return default;

            var root = await document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var token = root.FindToken(span.Start);
            var variableDeclaration = token.Parent.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (variableDeclaration != null) return (document, variableDeclaration);

            var fieldDeclaration = token.Parent.FirstAncestorOrSelf<FieldDeclarationSyntax>();
            if (fieldDeclaration == null) return default;

            if (!token.IsKind(SyntaxKind.CommaToken)
                && !token.IsKind(SyntaxKind.SemicolonToken)) return default;

            var movedToken = token.GetPreviousToken();
            if (movedToken == default) return default;

            var fieldVariableDeclaration = movedToken.Parent.FirstAncestorOrSelf<VariableDeclaratorSyntax>();
            if (fieldVariableDeclaration == null)
                return default;
            else
                return (document, fieldVariableDeclaration);
        }
    }
}
