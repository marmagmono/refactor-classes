using Microsoft.CodeAnalysis;
using System;

namespace RefactorClasses.ClassMembersModifications
{
    public static class AnalysedDeclarationExtensions
    {
        public static IFieldSymbol[] AsFieldArray(this AnalysedDeclaration declaration)
        {
            switch (declaration)
            {
                case FieldDeclaration fd: return new IFieldSymbol[] { fd.Field };
                default: return Array.Empty<IFieldSymbol>();
            }
        }

        public static IPropertySymbol[] AsPropertyArray(this AnalysedDeclaration declaration)
        {
            switch (declaration)
            {
                case PropertyDeclaration pd: return new IPropertySymbol[] { pd.Property };
                default: return Array.Empty<IPropertySymbol>();
            }
        }
    }
}
