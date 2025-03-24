// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 48

using _Script.Character.PlayerRank;
using _Script.Enemy.EnemyData;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Map.MapLoadContext.ContextInstance
{
    public class EnemyLoadContextInstance : MapLoadContextInstance
    {
        public GameObject[] MonsterPrefabs { get; }

        public EnemyLoadContextInstance(NiRank mapRank, string mapName, RewardContext.RewardContext reward, 
            GameObject[] monsterPrefabs) : base(mapRank, mapName, reward)
        {
            MonsterPrefabs = new GameObject[monsterPrefabs.Length];
            for (int i = 0; i < monsterPrefabs.Length; i++)
            {
                MonsterPrefabs[i] = monsterPrefabs[i];
            }
        }
    }
}