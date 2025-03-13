// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 10

using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Map.MapLoadContext;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.MapLoadContext.RewardContext;
using _Script.Map.MapLoadContext.Scriptable;
using _Script.Utilities;
using UnityEngine;

namespace _Script.Map.MapManager
{
    
    /// <summary>
    /// Holds the data for this game instance
    /// </summary>
    public class MapManager : PersistentSingleton<MapManager>
    {
        /// <summary>
        /// Map - [Map a] - [Map c] - [Map d] - [Map b] - [Map e]
        ///     - [Map b]           - [Map e]
        /// Multiple maps as options for each level
        /// </summary>
        private Queue<MapLoadContextInstance []> _currentMaps;
        
        [SerializeField] private BossMapLoadContext [] bossMaps;
        
        [SerializeField] private MonsterMapLoadContext [] monsterMaps;
        
        [SerializeField] private int miniMapCount = 3;
            
        [SerializeField] private RewardDataBase _rewardDataBase;


        public void StartGame()
        {
            GenerateGameMaps();
            
        }
        
        
        public void GenerateGameMaps()
        {
            //First Round
            //Generate 
            for(var i = miniMapCount - 1; i >= 0; i--)
            {
                var maps = GenerateMapsForALevel();
                if (maps == null)
                {
                    throw new System.Exception("Maps are null");
                }
                _currentMaps.Enqueue(maps);
            }
            
            if (_currentMaps.Count == 0)  throw new System.Exception("Maps are null");
            var boss = GenerateBossMap();
            if (boss == null)
            {
                throw new System.Exception("Boss is null");
            }
            _currentMaps.Enqueue(boss);
        }

        private MapLoadContextInstance[] GenerateBossMap()
        {
            var boss = RandomUtils.GetRandomUniqueItems(bossMaps, 1);
            return new [] {MapFactory.Create(boss[0], new RewardContext(GetRandomUniqueReward()))};
        }
        
        private  MapLoadContextInstance[] GenerateMapsForALevel()
        {
            //Each level has 2 options out of all the maps
            var maps = RandomUtils.GetRandomUniqueItems(monsterMaps, 2);
            //decide get unique reward type
            //random bool
            if (Random.value > 0.5f)
            {
                var map1 = MapFactory.Create(maps[0], new RewardContext(GetRandomEquipment()));
                var map2 = MapFactory.Create(maps[1], new RewardContext(GetRandomSupply()));
                return new [] {map1, map2};
            }
            else
            {
                var map1 = MapFactory.Create(maps[0], new RewardContext(GetRandomSupply()));
                var map2 = MapFactory.Create(maps[1], new RewardContext(GetRandomEquipment()));
                return new[] { map1, map2 };
            }
        }
        
        public List<ItemData> GetRandomEquipment()
        {
            return RandomUtils.GetRandomUniqueItems(_rewardDataBase.EquipmentRewards, 3);
        }
        
        public List<ItemData> GetRandomSupply()
        {
            return RandomUtils.GetRandomUniqueItems(_rewardDataBase.SupplyRewards, 3);
        }
        
        public List<ItemData> GetRandomUniqueReward()
        {
            return RandomUtils.GetRandomUniqueItems(_rewardDataBase.EquipmentRewards, 3);
        }
        
    }
}