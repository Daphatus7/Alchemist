using _Script.Character.PlayerStat;

namespace _Script.Character.PlayerStateMachine
{
    public class PlayerStaminaState : PlayerState
    {


        public override PlayerStateFlagType StateFlagType => PlayerStateFlagType.Exhaustion;        
        public PlayerStaminaState(IPlayerStatsManagerHandler playerStatsManager) : base(playerStatsManager, StatType.Stamina)
        {
        }
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