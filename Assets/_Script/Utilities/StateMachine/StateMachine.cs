// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 19

namespace _Script.Utilities.StateMachine
{
    public class StateMachine
    {
        private IState _currentState;

        public void SetState(IState newState)
        {
            // Exit the current state if there is one
            _currentState?.Exit();

            // Set the new state
            _currentState = newState;

            // Enter the new state
            _currentState.Enter();
        }

        public void UpdateState()
        {
            // Update the current state
            _currentState?.Update();
        }
    }
}