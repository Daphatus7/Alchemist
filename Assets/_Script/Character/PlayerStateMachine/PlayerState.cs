// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 27

using _Script.Character.PlayerStat;
using _Script.Utilities.StateMachine;

namespace _Script.Character.PlayerStateMachine
{
    public enum PlayerStateFlagType
    {
        Hunger,
        Insanity,
        Exhaustion,
    }
    
    public abstract class PlayerState : IState
    {
        public abstract PlayerStateFlagType StateFlagType { get; }
        public StatType ObservedStat { get; }
        protected IPlayerStatsManagerHandler PlayerStatsManager;

        public abstract void Enter();
        public abstract void UpdateState();
        public abstract void Exit();

        public void CleanUp()
        {
            PlayerStatsManager.GetStat(ObservedStat).onBelowThreshold -= Enter;
            PlayerStatsManager.GetStat(ObservedStat).onAboveThreshold -= Exit;
        }

        // Modified constructor with observedStat as a parameter.
        protected PlayerState(IPlayerStatsManagerHandler playerStatsManager, StatType observedStat)
        {
            PlayerStatsManager = playerStatsManager;
            ObservedStat = observedStat;
            PlayerStatsManager.GetStat(ObservedStat).onBelowThreshold += Enter;
            PlayerStatsManager.GetStat(ObservedStat).onAboveThreshold += Exit;
        }
    }
    

    
}