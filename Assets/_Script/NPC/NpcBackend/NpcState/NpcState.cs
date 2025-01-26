// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 36

using _Script.Utilities.StateMachine;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcState
{
    public abstract class NpcState : MonoBehaviour, IState
    {
        
        public void Initialize()
        {
        }
        
        public void Enter()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateState()
        {
            throw new System.NotImplementedException();
        }

        public void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
}