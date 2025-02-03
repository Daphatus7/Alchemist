using _Script.Character.PlayerStat;

namespace _Script.Character.PlayerStateMachine
{
    public class PlayerSanityState : PlayerState
    {
   
        public PlayerSanityState(IPlayerStatsManagerHandler playerStatsManager) : base(playerStatsManager, StatType.Sanity)
        {
        }
        public override PlayerStateFlagType StateFlagType => PlayerStateFlagType.Insanity;
        public override void Enter()
        {
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