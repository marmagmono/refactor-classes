using System;
using System.Threading.Tasks;

namespace RefactorClasses.Analysis.Test
{
    public static class Test
    {
        public static void Something()
        {
            Console.Out.WriteLine("dswwww");
        }
    }

    public class Test1
    {
        public void MyMethod(int a, int b)
        {
            int u = a;
            u = a;
        }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class StateMachineAttribute : Attribute
    {
        public Type StateType { get; set; }

        public Type TriggerType { get; set; }

        public Type ContextType { get; set; }
    }

    public class TriggerBase { }

    public class StateBase { }


    [StateMachine(ContextType = typeof(ContextBase), StateType = typeof(StateBase), TriggerType = (typeof(TriggerBase)))]
    public class StateMachineImpl
    {
        public void DoSomething(
            int a,
            Test1 testClass, string fdeee)
        {
        }

        public System.Threading.Tasks.Task AsyncOperationsSupport(int a, float b)
        {
            return Task.CompletedTask;
        }

        public async Task TaskMethod(int a, float b)
        {
            return;
        }

        public async Task<int> TaskMethodReturningSomething(int a, float b)
        {
            return 10;
        }
    }

    internal class ContextBase
    {
    }

    internal class SomethingBase { }

    internal class SomethingInheriting : SomethingBase { }

    internal class OtherThingInheriting : SomethingBase { }

    internal class ThirdThingInheriting : SomethingBase { }
}
