using System;
using System.Threading.Tasks;
using RefactorClasses.StateSample.Attributes;

namespace RefactorClasses.StateSample
{


    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
        }

        // Wrapper for methods - just proxy or something doing actual action.

        private interface IWorker
        {
            void DoSomething();

            Task<int> ComputeSum(int a, int b);
        }

        // what should it do ?
        // Compose different options for ?
        // prevent reentrancy
        public enum ActorOptions
        {
            // Producer / consumer implementation - TPL, channels ?
            // definition mechanism
            // error handling - ignore not matched things, throw ?
            // trigger naming convention
            // computation initiation ?
            // context ?
        }

        /*
         * - Find all "ActorLike" attributes - extend it with interface to be wrapped ?
         * - Find all StateConfiguration helpers
         * - enforce state configuration helpers are static
         * - generate good processor ?
         * - how to return something from function - task based - via task completion source. Other ones ?
         * - how to handle side effects - on state enter / leave
         * - TODO: determine dispatch mechanism ?
         * - TODO: how to push .net events into it - make it implement Init / Dispose by forcing it to implement an interface ?
         * - TODO: Notify state changes ?
         * - TODO: Additional dependencies for notifying state changes etc. in the main class ?
         * - TODO: side effects / waiting for tasks ?
         */

        [ExplicitlyStated(
            ActionType = typeof(TriggerBase),
            StateType = typeof(StateBase),
            StateContextType = typeof(SomethingContext))]
        [StateConfiguration(ForState = typeof(InitialState), ConfigurationClass = typeof(InitialStateConfiguration))]
        private partial class SomethingStateful
        {
            private static ActionResult<StateBase> DoSomething(InitialState s, DoSomethingTrigger doSomething)
            {
                return ActionResult<StateBase>.StayInState(s);
            }
        }

        private partial class SomethingStateful
        {
            private StateBase currentState;

            // TODO: notify that state has changed ?

            // Internal state vs public state
            // Wrapper via queue or no wrapper ?
            // Inject all dependencies in order to create a context ?

            public void DoSomething(string parameter, int value)
            {
                Dispatch(new DoSomethingTrigger(parameter, value));
            }

            private void Dispatch(TriggerBase trigger)
            {
                // either enqueue or process ?
            }

            private static ActionResult<StateBase> Process(StateBase currentState, TriggerBase trigger)
            {
                switch (currentState)
                {
                    case InitialState initialState:
                        break;
                }
            }

            private static ActionResult<StateBase> HandleInitialState(InitialState initialState, TriggerBase trigger)
            {
                switch(trigger)
                {
                    case DoSomethingTrigger doSomethingTrigger: return InitialStateConfiguration.DoSomething();
                }
            }
        }

        private class SomethingContext { }

        private class StateBase { }

        private class InitialState : StateBase { }

        private class ComputationState : StateBase { }

        private class DoneState : StateBase { }

        private class TriggerBase  { }

        private class DoSomethingTrigger : TriggerBase
        {
            public string Parameter { get; }

            public int Value { get; }

            public DoSomethingTrigger(string parameter, int value)
            {
                Parameter = parameter;
                Value = value;
            }
        }
    }
}
