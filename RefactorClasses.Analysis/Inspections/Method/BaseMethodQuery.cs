using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Method
{
    public class BaseMethodQuery
    {
        private readonly BaseMethodDeclarationSyntax syntax;

        public BaseMethodQuery(BaseMethodDeclarationSyntax syntax)
        {
            this.syntax = syntax;
        }

        public bool IsStatic() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));

        public bool IsPartial() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

        public bool IsAbstract() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword));

        public bool IsSealed() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword));

        public bool IsOverride() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.OverrideKeyword));

        public bool IsNew() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.NewKeyword));

        public bool IsPublic() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));

        public bool IsInternal() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.InternalKeyword));

        public bool IsProtected() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.ProtectedKeyword));

        public bool IsPrivate() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.PrivateKeyword));

        public bool IsVirtual() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.VirtualKeyword));

        public bool IsAsync() => syntax.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));

        public bool HasParameters() => syntax.ParameterList.Parameters.Count > 0;

        public bool IsArrowBody() => syntax.ExpressionBody != null;
    }
}
