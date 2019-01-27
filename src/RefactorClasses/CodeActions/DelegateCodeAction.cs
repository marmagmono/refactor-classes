using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RefactorClasses.CodeActions
{
    public class DelegateCodeAction : CodeAction
    {
        private Func<CancellationToken, Task<Document>> generateDocument;
        private string title;

        public DelegateCodeAction(string title, Func<CancellationToken, Task<Document>> generateDocument)
        {
            this.title = title;
            this.generateDocument = generateDocument;
        }

        public override string Title => title;

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken) =>
            generateDocument(cancellationToken);
    }
}
