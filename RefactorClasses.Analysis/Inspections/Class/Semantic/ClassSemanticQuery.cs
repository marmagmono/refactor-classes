using System;
using System.Collections;
using System.Collections.Generic;
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

