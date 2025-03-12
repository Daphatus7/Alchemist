// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 09

using _Script.Character.PlayerRank;
using _Script.Enemy.EnemyData;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Map.MapLoadContext.ContextInstance
{
    public class BossLoadContextInstance : MapLoadContextInstance
    {
        
        private EnemyData _bossData; public EnemyData BossData => _bossData;
        
        public BossLoadContextInstance(NiRank mapRank, string mapName, RewardContext.RewardContext reward,
            EnemyData bossData) : base(mapRank, mapName, reward)
        {
            _bossData = bossData;
        }
    }
}