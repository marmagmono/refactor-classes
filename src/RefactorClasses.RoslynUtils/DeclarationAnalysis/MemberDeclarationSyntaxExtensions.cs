using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class MemberDeclarationSyntaxExtensions
    {
        public static bool IsIndexer(this MemberDeclarationSyntax m) =>
            m is IndexerDeclarationSyntax;

        public static bool IsField(this MemberDeclarationSyntax m) =>
            m is FieldDeclarationSyntax;

        public static bool IsProperty(this MemberDeclarationSyntax m) =>
            m is PropertyDeclarationSyntax;

        public static bool IsEvent(this MemberDeclarationSyntax m) =>
            m is EventFieldDeclarationSyntax // like: event EventHandler ev;
            || m is EventDeclarationSyntax; // event with add and remove parts
    }
}
