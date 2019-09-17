using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace RefactorClasses.Analysis.Inspections.Class.Semantic
{
    //public enum FindClosestMode
    //{
    //    Forward, Backward
    //}

    public class ClassSemanticQuery
    {
        private readonly SemanticModel model;
        private readonly ClassInspector inspector;

        public ClassSemanticQuery(
            SemanticModel model,
            ClassInspector inspector)
        {
            this.model = model;
            this.inspector = inspector;
        }

        public bool TryFindFirstAttributeMatching(
            string attributeClassName,
            out AttributeData attributeSymbol,
            string attributeNamespace = "")
        {
            attributeSymbol = null;

            var classSymbol = this.model.GetDeclaredSymbol(this.inspector.Syntax);
            if (classSymbol == null)
            {
                return false;
            }

            var firstMatchingAttribute = classSymbol
                .GetAttributes()
                .Where(ad =>
                    ad.AttributeClass.Name == attributeClassName
                    && (string.IsNullOrEmpty(attributeNamespace)
                        || ad.AttributeClass.ContainingNamespace.Name == attributeNamespace))
                .FirstOrDefault();

            if (firstMatchingAttribute != null)
            {
                attributeSymbol = firstMatchingAttribute;
                return true;
            }

            attributeSymbol = null;
            return false;
        }

        // TODO: Find fields / properties assigned in constructor ?
        // TODO: Find closest field, closest property, or any of the two ?

        //public IEnumerable<FieldInfo> GetFields(Func<FieldInfo, bool> predicate)
        //{
        //}

        //public IEnumerable<PropertyInfo> GetProperties(Func<PropertyInfo, bool> predicate)
        //{
        //}

        // GetClass dependencies ?

        public void FindFieldsAndPropertiesAssignedInConstructor()
        {
        }

        //public static void FindClosest(
        //    FieldOrProperty fieldOrProperty,
        //    IReadOnlyCollection<FieldOrProperty> fieldOrProperties,
        //    FindClosestMode mode)
        //{
        //}
    }
}

