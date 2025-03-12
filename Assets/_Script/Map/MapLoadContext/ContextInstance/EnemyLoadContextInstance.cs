// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 48

using _Script.Character.PlayerRank;
using _Script.Enemy.EnemyData;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Map.MapLoadContext.ContextInstance
{
    public class EnemyLoadContextInstance : MapLoadContextInstance
    {
        private EnemyData[] _enemyData; public EnemyData[] EnemyData => _enemyData;
        
        public EnemyLoadContextInstance(NiRank mapRank, string mapName, RewardContext.RewardContext reward, 
            EnemyData[] enemyData) : base(mapRank, mapName, reward)
        {
            _enemyData = enemyData;
        }
    }
}