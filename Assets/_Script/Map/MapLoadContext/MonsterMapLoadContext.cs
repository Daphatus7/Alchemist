// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 27

using _Script.Enemy.EnemyData;
using UnityEngine;

namespace _Script.Map.MapLoadContext
{
    [CreateAssetMenu(fileName = "MonsterMapLoadContext", menuName = "MapLoadContext/MonsterMapLoadContext")]
    public class MonsterMapLoadContext : MapLoadContext
    {
        public EnemyData [] enemyData;
    }
}