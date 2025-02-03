using _Script.Character.PlayerStat;

namespace _Script.Character.PlayerStateMachine
{
    public class PlayerFoodState : PlayerState
    {
        public override PlayerStateFlagType StateFlagType => PlayerStateFlagType.Hunger;

        public PlayerFoodState(IPlayerStatsManagerHandler playerStatsManager)
            : base(playerStatsManager, StatType.Food)
        {
            
        }
        
        /// <summary>
        /// Enter hunger state.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
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