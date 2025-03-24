// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 27

using _Script.Enemy.EnemyData;
using UnityEngine;

namespace _Script.Map.MapLoadContext.Scriptable
{
    [CreateAssetMenu(fileName = "MonsterMapLoadContext", menuName = "MapLoadContext/MonsterMapLoadContext")]
    public class MonsterMapLoadContext : MapLoadContext
    {
        public GameObject [] monsterPrefabs;
        public override MapType MapType => MapType.Monster;
    }
}