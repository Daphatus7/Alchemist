// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 24

using System;
using UnityEngine;

namespace _Script.Utilities.StateMachine
{
    public abstract class MonStateMachine : MonoBehaviour
    {
        private IState [] _states;
        private IState _currentState; 

        public virtual void Awake()
        {
            _states = InitializeStateMachine();
        }

        protected abstract IState [] InitializeStateMachine();
        
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
            _currentState?.UpdateState();
        }
    }
}