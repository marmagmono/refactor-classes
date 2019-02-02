using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.ClassMembersModifications
{
    public abstract class AnalysedDeclaration
    {
        public abstract ISymbol Symbol { get; }
        public abstract TypeSyntax Type { get; }
        public abstract SyntaxToken Identifier { get; }
    }

    public sealed class PropertyDeclaration : AnalysedDeclaration
    {
        public IPropertySymbol Property { get; }
        public PropertyDeclarationSyntax Declaration { get; }

        public override ISymbol Symbol => Property;
        public override TypeSyntax Type => Declaration.Type;
        public override SyntaxToken Identifier => Declaration.Identifier;

        public PropertyDeclaration(IPropertySymbol property, PropertyDeclarationSyntax declaration)
        {
            Property = property;
            Declaration = declaration;
        }
    }

    public sealed class FieldDeclaration : AnalysedDeclaration
    {
        public IFieldSymbol Field { get; }
        public FieldDeclarationSyntax FullField { get; }
        public VariableDeclaratorSyntax Variable { get; }

        public override ISymbol Symbol => Field;
        public override TypeSyntax Type => FullField.Declaration.Type;
        public override SyntaxToken Identifier => Variable.Identifier;

        public FieldDeclaration(IFieldSymbol field, FieldDeclarationSyntax fullField, VariableDeclaratorSyntax variable)
        {
            Field = field;
            FullField = fullField;
            Variable = variable;
        }
    }
}