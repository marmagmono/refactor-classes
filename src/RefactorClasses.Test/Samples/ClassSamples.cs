using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.Test.Samples
{
    internal static class ClassSamples
    {
        public static string EmptyClass => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public Test() {}
    }
";

        public static string ClassWithTwoNonTrivialConstructors => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public int Prop1 { get; set; }
        public int Prop2 { get; set; }

        public Test(int prop1) => Prop1 = prop1;

        public Test(int prop1, int prop2)
        {
            Prop1 = prop1;
            Prop2 = prop2;
        }
    }
";

        public static string ClassWithoutNonStaticProperties => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public static int klo { get; set; }
        public static int klo4444 { get; set; }
    }
";
        public static string ClassWithIndexer => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public int this[int t]
        {
            get => t;
            set { }
        }

        public int klo { get; set; }
    }
";
        public static string ClassWithPropertyEvent => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public event EventHandler a
        {
            add { }
            remove { }
        }

        public int klo { get; set; }
    }
";

        public static string ClassWithEventField => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        public event EventHandler a;

        public int klo { get; set; }
    }
";
        public static string ClassWithField => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public class Test
    {
        private int a;

        public int klo { get; set; }
    }
";

        public static string PartialClass => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public partial class Test
    {
        public int klo { get; set; }
    }
";

        public static string StaticClass => @"
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Threading.Tasks;

    public static class Test
    {
        public static int klo { get; set; }
    }
";

    }
}
