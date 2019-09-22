﻿using System.Collections.Generic;
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

        private readonly List<TypeSyntax> baseTypes = new List<TypeSyntax>();
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
            this.properties.Add(new PropertyInfo(type.WithoutTrivia(), identifier));
            return this;
        }

        public RecordBuilder AddProperties(params (TypeSyntax type, string identifier)[] properties)
        {
            foreach (var (t, id) in properties)
            {
                AddProperty(t, GeneratorHelper.UppercaseFirstLetter(id));
            }

            return this;
        }

        public RecordBuilder AddBaseTypes(params TypeSyntax[] baseTypes)
        {
            this.baseTypes.AddRange(baseTypes);
            return this;
        }

        public ClassDeclarationSyntax Build()
        {
            var identifier = SF.Identifier(recordName);

            if (!modifiers.Any(m => m.IsKind(SyntaxKind.SealedKeyword)))
            {
                modifiers.Add(Modifiers.Sealed);
            }

            // base types -> markers
            var baseList = this.baseTypes.Count == 0 ?
                default(BaseListSyntax)
                : SF.BaseList(Tokens.Colon,
                    SF.SeparatedList(this.baseTypes.Select(t => SF.SimpleBaseType(t) as BaseTypeSyntax)));

            // TODO: rething trivia usage everywhere ?
            // TODO: switch between windows / linux EOL
            // TODO: Build properties with getter and uppercase name if needed
            // TODO: Allow choosing between this.property vs no this
            var generatedProperties = this.properties.Select(
                p => new PropertyBuilder(p.Type, p.Identifier)
                        .AddModifiers(Modifiers.Public)
                        .Build(PropertyBuilder.PropertyType.ReadonlyGet))
                .ToList();

            // Constructor
            var parameters = generatedProperties.Select(p =>
                GeneratorHelper.Parameter(
                    p.Type,
                    GeneratorHelper.LowercaseIdentifierFirstLetter(p.Identifier)));

            var body = generatedProperties.Select(prop =>
                SF.ExpressionStatement(
                    ExpressionGenerationHelper.SimpleAssignment(
                        prop.Identifier,
                        GeneratorHelper.LowercaseIdentifierFirstLetter(prop.Identifier)))
            );

            var generatedConstructor = new MethodBuilder(identifier)
                .Modifiers(Modifiers.Public)
                .Parameters(parameters.ToArray())
                .Body(SF.Block(body))
                .BuildConstructor();

            // Create actual declaration
            var members = generatedProperties
                .Select(p => p.WithSemicolonToken(Tokens.Semicolon))
                .Cast<MemberDeclarationSyntax>()
                .ToList();

            members.Add(generatedConstructor);

            return SF.ClassDeclaration(
                GeneratorHelper.EmptyAttributeList(),
                SF.TokenList(this.modifiers),
                identifier,
                default(TypeParameterListSyntax),
                baseList,
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
