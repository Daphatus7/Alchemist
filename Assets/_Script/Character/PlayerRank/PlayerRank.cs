// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 17

using _Script.Utilities.StateMachine;

namespace _Script.Character.PlayerRank
{
    public class PlayerRank : MyStateMachine
    {
        public PlayerRankEnum Rank => PlayerRankEnum.F;

    }
    
    public class PlayerRankState : IState
    {
        public virtual void Enter()
        {
            throw new System.NotImplementedException();
        }

        public virtual void Exit()
        {
            throw new System.NotImplementedException();
        }

        public virtual void UpdateState()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class PlayerRankE : PlayerRankState
    {
        public override void Enter()
        {
            base.Enter();
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void UpdateState()
        {
            base.UpdateState();
        }
    }
    
    
    public enum PlayerRankEnum
    {
        F,
        E,
        D,
        C,
        B,
        A,
        S,
        SS
    }
}