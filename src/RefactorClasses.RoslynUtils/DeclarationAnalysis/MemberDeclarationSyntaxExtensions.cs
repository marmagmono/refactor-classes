using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class MemberDeclarationSyntaxExtensions
    {
        public static bool IsIndexer(this MemberDeclarationSyntax m) =>
            m.IsKind(SyntaxKind.IndexerDeclaration);

        public static bool IsField(this MemberDeclarationSyntax m) =>
            m.IsKind(SyntaxKind.FieldDeclaration);

        public static bool IsProperty(this MemberDeclarationSyntax m) =>
            m.IsKind(SyntaxKind.PropertyDeclaration);

        public static bool IsEvent(this MemberDeclarationSyntax m) =>
            m.IsKind(SyntaxKind.EventFieldDeclaration) // like: event EventHandler ev;
            || m.IsKind(SyntaxKind.EventDeclaration); // event with add and remove parts
    }
}
