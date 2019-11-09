namespace RefactorClasses.StateSample
{
    public struct ActionResult<TState>
    {
        private enum ResultType
        {
            StayInState,
            Transition, // full transition with 
        }

        private readonly ResultType resultType;
        private readonly TState nextState;

        private ActionResult(TState state, ResultType resultType)
        {
            this.nextState = state;
            this.resultType = resultType;
        }

        public static ActionResult<TState> Transition(TState state) =>
            new ActionResult<TState>(state, ResultType.Transition);

        public static ActionResult<TState> StayInState(TState state) =>
            new ActionResult<TState>(state, ResultType.StayInState);

        public bool IsTransition(out TState state)
        {
            if (this.resultType == ResultType.Transition)
            {
                state = this.nextState;
                return true;
            }

            state = default;
            return false;
        }

        public bool IsStayInState(out TState state)
        {
            if (this.resultType == ResultType.StayInState)
            {
                state = this.nextState;
                return true;
            }

            state = default;
            return false;
        }
    }
}
