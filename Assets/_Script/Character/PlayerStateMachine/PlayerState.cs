// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 27

using _Script.Utilities.StateMachine;

namespace _Script.Character.PlayerStateMachine
{
    public enum PlayerStateType
    {
        Hunger,
        Insanity,
        Exhaustion,
    }
    
    public abstract class PlayerState : IState
    {
        public abstract PlayerStateType StateType { get; }
        private PlayerStatsManager _playerStatsManager;
        public abstract void Enter();
        public abstract void UpdateState();
        public abstract void Exit();
        
        public void SetPlayerStatsManager(PlayerStatsManager context)
        {
            _playerStatsManager = context;
        }
    }
    
    public class PlayerFoodState : PlayerState
    {
        public override PlayerStateType StateType => PlayerStateType.Hunger;

        public override void Enter()
        {
            throw new System.NotImplementedException();
        }

        public override void UpdateState()
        {
            throw new System.NotImplementedException();
        }

        public override void Exit()
        {
            throw new System.NotImplementedException();
        }
    }
    
}