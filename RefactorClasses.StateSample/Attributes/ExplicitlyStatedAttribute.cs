using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.StateSample.Attributes
{
    // And Finally: Dispatcher ?

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ExplicitlyStatedAttribute : Attribute
    {
        public Type StateType { get; set; }

        public Type ActionType { get; set; }

        public Type StateContextType { get; set; }
    }
}
