using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.Analysis.DeclarationGeneration;

namespace RefactorClasses.Analysis.Generators
{
    using SF = SyntaxFactory;

    /// <summary>
    /// Utility class for building simple record like classes, i.e. a class
    /// with public readonly properties and no methods.
    /// </summary>
    public sealed class RecordBuilder
    {
        private readonly string recordName;
        private readonly List<SyntaxToken> modifiers = new List<SyntaxToken>();
        private readonly List<PropertyInfo> properties = new List<PropertyInfo>();

        public RecordBuilder(string recordName)
        {
            this.recordName = recordName;
        }

        public RecordBuilder AddModifiers(params SyntaxToken[] modifier)
        {
            modifiers.AddRange(modifier);
            return this;
        }

        public RecordBuilder AddProperty(TypeSyntax type, string identifier)
        {
            this.properties.Add(new PropertyInfo(type, identifier));
            return this;
        }

        public RecordBuilder AddProperties(params (TypeSyntax type, string identifier)[] properties)
        {
            foreach (var (t, id) in properties)
            {
                AddProperty(t, id);
            }

            return this;
        }

        public ClassDeclarationSyntax Build()
        {
            var identifier = SF.Identifier(recordName);

            if (!modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)))
            {
                modifiers.Add(Modifiers.Sealed);
            }

            // TODO: rething trivia usage everywhere ?
            // TODO: switch between windows / linux EOL
            var generatedProperties = this.properties.Select(
                p =>
                    SF.PropertyDeclaration(p.Type, p.Identifier)
                    .WithSemicolonToken(Tokens.Semicolon)
                    .WithTrailingTrivia(SF.ElasticCarriageReturnLineFeed))
                .ToList();

            // Constructor
            var parameters = generatedProperties.Select(p =>
                GeneratorHelper.Parameter(
                    p.Type,
                    GeneratorHelper.LowercaseIdentifierFirstLetter(p.Identifier)));

            var body = generatedProperties.Select(prop =>
                SyntaxFactory.ExpressionStatement(
                    ExpressionGenerationHelper.SimpleAssignment(
                        prop.Identifier,
                        GeneratorHelper.LowercaseIdentifierFirstLetter(prop.Identifier)))
            );

            var generatedConstructor = new MethodBuilder(identifier)
                .Modifiers(Modifiers.Public)
                .Parameters()
                .Body(SF.Block(body))
                .BuildConstructor();

            // Create actual declaration
            var members = new List<MemberDeclarationSyntax>(generatedProperties);
            members.Add(generatedConstructor);

            return SF.ClassDeclaration(
                GeneratorHelper.EmptyAttributeList(),
                SF.TokenList(modifiers),
                identifier,
                default(TypeParameterListSyntax),
                default(BaseListSyntax),
                GeneratorHelper.EmptyParameterConstraintList(),
                SF.List(members));
        }

        private class PropertyInfo
        {
            public TypeSyntax Type { get; }

            public string Identifier { get; }

            public PropertyInfo(TypeSyntax type, string identifier)
            {
                Type = type;
                Identifier = identifier;
            }
        }
    }
}
