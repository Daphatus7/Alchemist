// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 57

using _Script.Enemy.EnemyData;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Map.MapLoadContext.Scriptable
{
    [CreateAssetMenu(fileName = "BossMapLoadContext", menuName = "MapLoadContext/BossMapLoadContext")]
    public class BossMapLoadContext : MapLoadContext
    {
        public EnemyData bossData;
        public ItemData [] uniqueReward;
        public override MapType MapType => MapType.Boss;
    }
}