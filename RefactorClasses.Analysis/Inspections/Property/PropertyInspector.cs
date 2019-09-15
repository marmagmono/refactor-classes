using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Property
{
    public class PropertyInspector
    {
        private readonly PropertyDeclarationSyntax syntax;

        public PropertyInspector(PropertyDeclarationSyntax syntax)
        {
            this.syntax = syntax;
        }

        public string Name => syntax.Identifier.WithoutTrivia().ValueText;

        public bool IsReadOnly() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ReadOnlyKeyword));

        public bool IsAbstract() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));

        public bool IsSealed() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword));

        public bool IsPublic() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

        public bool IsInternal() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword));

        public bool IsProtected() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));

        public bool IsPrivate() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

        public bool HasSetter() => syntax.AccessorList.Accessors.Any(m => m.IsKind(SyntaxKind.SetAccessorDeclaration));

        public bool HasGetter() => syntax.AccessorList.Accessors.Any(m => m.IsKind(SyntaxKind.GetAccessorDeclaration));
    }
}
