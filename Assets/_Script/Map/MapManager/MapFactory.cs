// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 06

using _Script.Character.PlayerRank;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.MapLoadContext.RewardContext;

namespace _Script.Map.MapManager
{
    public class MapFactory
    {
        private const NiRank MapRank = NiRank.F;

        public static MapLoadContextInstance Create
            (MapLoadContext.Scriptable.MapLoadContext mapLoadContext,
                RewardContext rewardContext
                )
        {
            switch (mapLoadContext.MapType)
            {
                case MapLoadContext.Scriptable.MapType.Monster:
                    return new EnemyLoadContextInstance(MapRank, mapLoadContext.mapName, rewardContext,
                        ((MapLoadContext.Scriptable.MonsterMapLoadContext) mapLoadContext).monsterPrefabs);
                case MapLoadContext.Scriptable.MapType.Boss:
                    return new BossLoadContextInstance(MapRank, mapLoadContext.mapName, rewardContext,
                        ((MapLoadContext.Scriptable.BossMapLoadContext) mapLoadContext).bossData);
                case MapLoadContext.Scriptable.MapType.Town:
                    return new TownLoadContextInstance(MapRank, mapLoadContext.mapName, rewardContext)
                    {
                        IsCompleted = true
                    };
                default:
                    return null;
            }
        }
    }
}