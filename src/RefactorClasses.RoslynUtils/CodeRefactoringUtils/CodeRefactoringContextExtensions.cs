using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeRefactorings;
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
    }
}
