using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Class
{
    public class ClassQuery
    {
        private readonly ClassDeclarationSyntax syntax;

        public ClassQuery(ClassDeclarationSyntax syntax)
        {
            this.syntax = syntax;
        }

        public bool IsStatic() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public bool IsPartial() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

        public bool IsAbstract() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));

        public bool IsSealed() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword));

        public bool IsPublic() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

        public bool IsInternal() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword));

        public bool IsProtected() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));

        public bool IsPrivate() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

        public bool HasConstructors() => syntax.Members.Any(m => m is ConstructorDeclarationSyntax);

        public bool HasEvents() => syntax.Members.Any(m => m is EventDeclarationSyntax);
    }
}
