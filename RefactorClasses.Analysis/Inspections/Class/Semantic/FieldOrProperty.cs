namespace RefactorClasses.Analysis.Inspections.Class.Semantic
{
    public abstract class FieldOrProperty
    {
        public static FieldWrapper FieldWrapper(FieldInfo fieldSymbol) => new FieldWrapper(fieldSymbol);

        public static PropertyWrapper PropertyWrapper(PropertyWrapper propertySymbol) => new PropertyWrapper(propertySymbol);
    }

    public sealed class FieldWrapper : FieldOrProperty
    {
        public FieldInfo FieldSymbol { get; }

        public FieldWrapper(FieldInfo fieldSymbol)
        {
            FieldSymbol = fieldSymbol;
        }
    }

    public sealed class PropertyWrapper : FieldOrProperty
    {
        public PropertyWrapper PropertySymbol { get; }

        public PropertyWrapper(PropertyWrapper propertySymbol)
        {
            PropertySymbol = propertySymbol;
        }
    }
}

