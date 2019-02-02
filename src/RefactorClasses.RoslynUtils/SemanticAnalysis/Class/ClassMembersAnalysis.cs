using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RefactorClasses.RoslynUtils.DeclarationAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RefactorClasses.RoslynUtils.SemanticAnalysis.Class
{
    using FieldList = IReadOnlyCollection<FieldInfo>;
    using PropertyList = IReadOnlyCollection<PropertyInfo>;

    public class FieldInfo
    {
        public FieldInfo(IFieldSymbol symbol, VariableDeclaratorSyntax variable)
        {
            Symbol = symbol;
            Variable = variable;
        }

        public IFieldSymbol Symbol { get; }

        public VariableDeclaratorSyntax Variable { get; }
    }

    public class PropertyInfo
    {
        public PropertyInfo(IPropertySymbol symbol, PropertyDeclarationSyntax declaration)
        {
            Symbol = symbol;
            Declaration = declaration;
        }

        public IPropertySymbol Symbol { get; }

        public PropertyDeclarationSyntax Declaration { get; }
    }

    public delegate bool AcceptFieldPredicate(in FieldInfo field);
    public delegate bool AcceptPropertyPredicate(in PropertyInfo field);

    public sealed class ClassMembersAnalysis
    {
        private const int NoElementDistance = int.MaxValue;

        private readonly ClassDeclarationSyntax classDeclarationSyntax;
        private readonly SemanticModel model;

        private FieldList resolvedFields;
        private PropertyList resolvedProperties;

        public ClassMembersAnalysis(
            ClassDeclarationSyntax classDeclarationSyntax,
            SemanticModel model)
        {
            this.classDeclarationSyntax = classDeclarationSyntax;
            this.model = model;
        }

        private ClassMembersAnalysis(
            ClassDeclarationSyntax classDeclarationSyntax,
            SemanticModel model,
            FieldList fieldList,
            PropertyList propertyList) =>
            (this.classDeclarationSyntax, this.model, resolvedFields, resolvedProperties) =
                (classDeclarationSyntax, model, fieldList, propertyList);

        public FieldList Fields
        {
            get
            {
                EnsureFieldsResolved();
                return resolvedFields;
            }
        }

        public PropertyList Properties
        {
            get
            {
                EnsurePropertiesResolved();
                return resolvedProperties;
            }
        }

        public ClassMembersAnalysis WithFilteredProperties(
            AcceptFieldPredicate fieldPredicate,
            AcceptPropertyPredicate propertyPredicate)
        {
            EnsureFieldsResolved();
            EnsurePropertiesResolved();

            var filteredFields = new List<FieldInfo>(resolvedFields.Count);
            foreach (var f in resolvedFields)
            {
                if (fieldPredicate(in f)) filteredFields.Add(f);
            }

            var filteredProperties = new List<PropertyInfo>(resolvedProperties.Count);
            foreach (var p in resolvedProperties)
            {
                if (propertyPredicate(in p)) filteredProperties.Add(p);
            }

            return new ClassMembersAnalysis(classDeclarationSyntax, model, filteredFields, filteredProperties);
        }

        /// <summary>
        /// Tries to find closest field or proerpty to the given one.
        /// </summary>
        /// <remarks>It is assumed that all spans of analysed members do not overlap.</remarks>
        /// <param name="fieldOrProperty">Given field or property.</param>
        /// <returns>Closest symbol and a flag indicating whether the given symbol is before or after the found one.</returns>
        public (ISymbol symbol, bool isBeforeFoundSymbol) GetClosestFieldOrProperty(ISymbol fieldOrProperty)
        {
            EnsureFieldsResolved();
            EnsurePropertiesResolved();

            var fieldOrPropertySyntax = fieldOrProperty?.DeclaringSyntaxReferences.FirstOrDefault();
            if (fieldOrPropertySyntax == null) return (null, false);

            var start = fieldOrPropertySyntax.Span.Start;
            var end = fieldOrPropertySyntax.Span.End;

            var (fi, fdist) = FindClosestElement(
                resolvedFields,
                ShouldSkipField,
                (in FieldInfo f) => Math.Max(start - f.Variable.Span.End, f.Variable.Span.Start - end));

            var (pi, pdist) = FindClosestElement(
                resolvedProperties,
                ShouldSkipProp,
                (in PropertyInfo p) => Math.Max(start - p.Declaration.Span.End, p.Declaration.Span.Start - end));

            switch ((fdist, pdist))
            {
                case var t when t.fdist == NoElementDistance && t.pdist == NoElementDistance:
                    return (null, false); // No element found
                case var t when t.fdist == NoElementDistance:
                    return (pi.Symbol, IsBefore(pi.Declaration)); // No field found
                case var t when t.pdist == NoElementDistance:
                    return (fi.Symbol, IsBefore(fi.Variable)); // No property found
                case var t:
                    return (t.fdist < t.pdist) ?
                        ((ISymbol)fi.Symbol, IsBefore(fi.Variable))
                        : (pi.Symbol, IsBefore(pi.Declaration));
            }

            bool ShouldSkipField(in FieldInfo finfo) => finfo.Variable.Span.Start == fieldOrPropertySyntax.Span.Start;
            bool ShouldSkipProp(in PropertyInfo pinfo) => pinfo.Declaration.Span.Start == fieldOrPropertySyntax.Span.Start;
            bool IsBefore(CSharpSyntaxNode node) => fieldOrPropertySyntax.Span.End < node.Span.Start;
        }


        private void EnsureFieldsResolved()
        {
            if (resolvedFields != null) return;

            var declarations = ClassDeclarationSyntaxAnalysis.GetFieldVariableDeclarations(classDeclarationSyntax);
            List<FieldInfo> result = new List<FieldInfo>(declarations.Count());
            foreach(var d in declarations)
            {
                var symbol = model.GetDeclaredSymbol(d) as IFieldSymbol;
                if (symbol != null)
                {
                    result.Add(new FieldInfo(symbol, d));
                }
            }

            resolvedFields = result as FieldList;
        }

        private void EnsurePropertiesResolved()
        {
            if (resolvedProperties != null) return;

            var declarations = ClassDeclarationSyntaxAnalysis.GetPropertyDeclarations(classDeclarationSyntax).ToList();
            List<PropertyInfo> result = new List<PropertyInfo>(declarations.Count());
            foreach (var d in declarations)
            {
                var symbol = model.GetDeclaredSymbol(d) as IPropertySymbol;
                if (symbol != null)
                {
                    result.Add(new PropertyInfo(symbol, d));
                }
            }

            resolvedProperties = result as PropertyList;
        }

        private static (T element, int distance) FindClosestElement<T>(
            IReadOnlyCollection<T> declarations,
            ShouldSkipDelegate<T> shouldSkip,
            GetDistanceDelegate<T> getDistance)
        {
            T selected = default(T);
            int minDistance = NoElementDistance;

            foreach (var d in declarations)
            {
                if (shouldSkip(in d)) continue;

                var dist = getDistance(in d);
                if (dist > 0 && dist < minDistance)
                {
                    minDistance = dist;
                    selected = d;
                }
            }

            return (selected, minDistance);
        }


        private delegate bool ShouldSkipDelegate<T>(in T val);
        private delegate int GetDistanceDelegate<T>(in T val);
    }
}
