using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.DeclarationGeneration;
using RefactorClasses.Analysis.Inspections.Class.Semantic;
using RefactorClasses.Analysis.Inspections.Flow;
using RefactorClasses.Analysis.Inspections.Method;
using RefactorClasses.Analysis.Inspections.Property;

namespace RefactorClasses.Analysis.Inspections.Class
{
    using SF = SyntaxFactory;

    public class ClassInspector
    {
        private readonly ClassDeclarationSyntax syntax;

        public ClassInspector(ClassDeclarationSyntax syntax)
        {
            this.syntax = syntax;
        }

        public static ClassInspector Create(ClassDeclarationSyntax syntax) => new ClassInspector(syntax);

        public string Name => syntax.Identifier.WithoutTrivia().ValueText;

        public TestFlow Check(Func<ClassQuery, bool> test)
        {
            return new TestFlow(test(new ClassQuery(this.syntax)));
        }

        public TestFlow CheckConstructors(Func<IEnumerable<MethodInspector>, bool> test)
        {
            var ci = GetMembers<ConstructorDeclarationSyntax>().Select(m => new MethodInspector(m));
            return new TestFlow(test(ci));
        }

        public IEnumerable<MethodInspector> FindMatchingConstructors(Func<MethodInspector, bool> test)
        {
            return GetMembers<ConstructorDeclarationSyntax>()
                .Select(m => new MethodInspector(m))
                .Where(test);
        }

        public TestFlow CheckMethods(Func<IEnumerable<MethodInspector>, bool> test)
        {
            var ci = GetMembers<MethodDeclarationSyntax>().Select(m => new MethodInspector(m));
            return new TestFlow(test(ci));
        }

        public IEnumerable<MethodInspector> FindMatchingMethods(Func<MethodInspector, bool> test)
        {
            return GetMembers<MethodDeclarationSyntax>()
                .Select(m => new MethodInspector(m))
                .Where(test);
        }

        public TestFlow CheckProperties(Func<IEnumerable<PropertyInspector>, bool> test)
        {
            var ci = GetMembers<PropertyDeclarationSyntax>().Select(m => new PropertyInspector(m));
            return new TestFlow(test(ci));
        }

        public IEnumerable<PropertyInspector> FindMatchingProperties(Func<PropertyInspector, bool> test)
        {
            return GetMembers<PropertyDeclarationSyntax>()
                .Select(m => new PropertyInspector(m))
                .Where(test);
        }

        public TestFlow CheckAttributes(Func<IEnumerable<AttributeSyntax>, bool> test)
        {
            var attributes =
                this.syntax.AttributeLists.SelectMany(al => al.Attributes);
            return new TestFlow(test(attributes));
        }

        public IEnumerable<AttributeSyntax> FindMatchingAttributes(Func<AttributeSyntax, bool> test)
        {
            return this.syntax.AttributeLists
                .SelectMany(al => al.Attributes)
                .Where(a => test(a));
        }

        public ClassSemanticQuery CreateSemanticQuery(SemanticModel model) => new ClassSemanticQuery(model, this);

        // constructors
        // methods
        // operators
        // destructors
        // fields
        // events
        // properties

        // type

        public IdentifierNameSyntax Identifier => SF.IdentifierName(syntax.Identifier.WithoutTrivia());

        public ClassDeclarationSyntax Syntax => this.syntax;

        private IEnumerable<TMember> GetMembers<TMember>() =>
            this.syntax.Members
                .Where(m => m is TMember)
                .Cast<TMember>();
    }


}
