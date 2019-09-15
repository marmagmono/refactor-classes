using System;
using System.Collections.Generic;
using System.Text;

namespace RefactorClasses.Analysis.Inspections.Flow
{
    public readonly struct TestFlow
    {
        public readonly bool Passed;

        public TestFlow(bool result)
        {
            Passed = result;
        }

        public TestFlow And<T>(Func<TestFlow> f)
        {
            return Passed ? f() : this;
        }

        public TestFlow Or<T>(Func<TestFlow> f)
        {
            TestFlow otherRes = f();
            return new TestFlow(Passed || otherRes.Passed);
        }
    }
}
