// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 59

namespace _Script.Utilities.StateMachine
{
    public interface IState
    {
        /// <summary>
        /// Called when the state machine enters this state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Called every frame (or FixedUpdate) while this state is active.
        /// </summary>
        void Update();

        /// <summary>
        /// Called when the state machine exits this state.
        /// </summary>
        void Exit();
    }
}