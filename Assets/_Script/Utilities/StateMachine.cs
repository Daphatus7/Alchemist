// Author : Peiyu Wang @ Daphatus
// 18 12 2024 12 51

namespace _Script.Utilities
{
    public interface IState
    {
        void Enter();
        void Exit();
        void Update();
    }

    public class StateMachine
    {
        private IState _currentState;

        public void ChangeState(IState newState)
        {
            _currentState?.Exit();
            _currentState = newState;
            _currentState.Enter();
        }

        public void Update()
        {
            _currentState?.Update();
        }
    }
}