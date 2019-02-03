using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace RefactorClasses.RoslynUtils.DeclarationAnalysis
{
    public static class ClassDeclarationSyntaxAnalysis
    {
        // TODO: Exclude properties like: IsReady => !string.IsNullOrEmpty(...)
        public static bool IsRecordLike(ClassDeclarationSyntax classDeclarationSyntax) =>
            !IsStatic(classDeclarationSyntax)
            && !IsPartial(classDeclarationSyntax)
            && !HasEvents(classDeclarationSyntax)
            && !HasFields(classDeclarationSyntax)
            && !HasIndexers(classDeclarationSyntax);

        public static bool CanBeDiscriminatedUnionBaseType(ClassDeclarationSyntax classDeclarationSyntax) =>
            !IsStatic(classDeclarationSyntax)
            && !IsPartial(classDeclarationSyntax)
            && !HasEvents(classDeclarationSyntax)
            && !HasFields(classDeclarationSyntax)
            && !HasNonAbstractProperties(classDeclarationSyntax)
            && !HasIndexers(classDeclarationSyntax)
            && IsAbstract(classDeclarationSyntax);

        public static bool IsAbstract(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Modifiers.Any(m => m.Kind() == SyntaxKind.AbstractKeyword);

        public static bool IsStatic(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Modifiers.Any(m => m.Kind() == SyntaxKind.StaticKeyword);

        public static bool IsPartial(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Modifiers.Any(m => m.Kind() == SyntaxKind.PartialKeyword);

        public static bool HasIndexers(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Members.Any(MemberDeclarationSyntaxExtensions.IsIndexer);

        public static bool HasFields(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Members.Any(MemberDeclarationSyntaxExtensions.IsField);

        public static bool HasEvents(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Members
                .Any(MemberDeclarationSyntaxExtensions.IsEvent); // event with add and remove parts

        public static bool HasProperties(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax.Members.Any(MemberDeclarationSyntaxExtensions.IsProperty);

        public static bool HasNonAbstractProperties(ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax
                .GetMembers<PropertyDeclarationSyntax>()
                .Where(p => !p.Modifiers.Any(m => m.IsKind(SyntaxKind.AbstractKeyword)))
                .Count() > 0;

        public static IEnumerable<PropertyDeclarationSyntax> GetPropertyDeclarations(ClassDeclarationSyntax classDeclarationSyntax) =>
            GetMembers<PropertyDeclarationSyntax>(classDeclarationSyntax);

        public static IEnumerable<FieldDeclarationSyntax> GetFieldDeclarations(ClassDeclarationSyntax classDeclarationSyntax) =>
            GetMembers<FieldDeclarationSyntax>(classDeclarationSyntax);

        public static IEnumerable<VariableDeclaratorSyntax> GetFieldVariableDeclarations(ClassDeclarationSyntax classDeclarationSyntax) =>
            GetMembers<FieldDeclarationSyntax>(classDeclarationSyntax).SelectMany(f => f.Declaration.Variables);

        public static IEnumerable<MethodDeclarationSyntax> GetOverrideMethods(this ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax
                .GetMembers<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.OverrideKeyword)));

        public static IEnumerable<PropertyDeclarationSyntax> GetOverrideProperties(this ClassDeclarationSyntax classDeclarationSyntax) =>
            classDeclarationSyntax
                .GetMembers<PropertyDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.OverrideKeyword)));

        public static (bool conditionTrue, ConstructorDeclarationSyntax nonTrivialConstructor)
            HasAtMostOneNoneTrivialConstructor(ClassDeclarationSyntax classDeclarationSyntax)
        {
            var constructors = GetConstructors(classDeclarationSyntax)?.ToList();
            if (constructors == null) return (false, null);

            switch (constructors)
            {
                case var c when c.Count > 2 :
                    return (false, null);

                case var c when c.Count == 2:
                    var emptyConstructorIdx = c.FindIndex(ConstructorDeclarationSyntaxAnalysis.IsEmpty);
                    if (emptyConstructorIdx == -1)
                    {
                        return (false, null);
                    }
                    else
                    {
                        var nonEmptyConstructor = c[(emptyConstructorIdx + 1) % 2];
                        return (true, nonEmptyConstructor);
                    }

                case var c when c.Count == 1:
                    return ConstructorDeclarationSyntaxAnalysis.IsEmpty(c[0]) ?
                        (true, null) : (true, c[0]);

                default:
                    return (true, null); // 0 constructors
            }
        }

        public static IEnumerable<ConstructorDeclarationSyntax> GetConstructors(
            ClassDeclarationSyntax classDeclarationSyntax) =>
                GetMembers<ConstructorDeclarationSyntax>(classDeclarationSyntax);

        public static IEnumerable<T> GetMembers<T>(this ClassDeclarationSyntax classDeclarationSyntax)
            where T : class =>
            classDeclarationSyntax?.Members
                .Where(m => m is T)
                .Cast<T>();

    }
}
