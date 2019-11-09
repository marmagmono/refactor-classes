using System;

namespace RefactorClasses.StateSample.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class StateConfigurationAttribute : Attribute
    {
        public Type ForState { get; set; }

        public Type ConfigurationClass { get; set; }
    }
}
