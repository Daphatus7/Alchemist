// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 10

using System.Collections.Generic;
using System.Linq;
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
        private readonly Queue<MapLoadContextInstance []> _allMaps = new();

        public bool IsLevelCompleted
        {
            get
            {
                return _currentMap.IsCompleted;
            }
        }

        private MapLoadContextInstance _currentMap;
        /// <summary>
        /// the map in which the player is currently in
        /// </summary>
        public MapLoadContextInstance CurrentMap
        {
            get
            {
                return _currentMap;
            }
            set
            {
                _currentMap = value;
            }
        }
        /// <summary>
        /// Maps for the next level
        /// </summary>
        public MapLoadContextInstance[] NextPossibleMaps
        {
            get
            {
                // Ensure there's at least two items in the queue.
                if (_allMaps.Count > 1)
                {
                    // Convert the queue to an array to access elements by index.
                    var mapsArray = _allMaps.ToArray();
                    return mapsArray[1]; // The item "next to" the first (head) element.
                }
                return null; // Or consider returning an empty array if that suits your design better.
            }
        }

        
        [SerializeField] private BossMapLoadContext [] bossMaps;
        
        [SerializeField] private MonsterMapLoadContext [] monsterMaps;

        [SerializeField] private TownMapLoadContext townMap;
        
        [SerializeField] private int miniMapCount = 3;
            
        [SerializeField] private RewardDataBase _rewardDataBase;
        
        /// <summary>
        /// Generate the game maps
        /// </summary>
        public void InitializeMaps()
        {
            //consider the town map
            var town = 
                MapFactory.Create(townMap, 
                    new RewardContext(
                        RandomUtils.GetRandomUniqueItems(_rewardDataBase.EquipmentRewards, 3).ToArray(),
                        RewardType.Equipment));
            _allMaps.Enqueue(new [] {town});
            CurrentMap = town;
            GenerateGameMaps();
        }
        
        private void LoadCurrentLevelMaps(int playerSelectedLevel)
        {
            if (_allMaps.Peek().Length <= playerSelectedLevel)
            {
                throw new System.Exception("Maps are null");
            }
            CurrentMap = _allMaps.Peek()[playerSelectedLevel];
        }
        
        private void GenerateGameMaps()
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
                _allMaps.Enqueue(maps);
            }
            
            if (_allMaps.Count == 0)  throw new System.Exception("Maps are null");
            var boss = GenerateBossMap();
            if (boss == null)
            {
                throw new System.Exception("Boss is null");
            }
            _allMaps.Enqueue(boss);
        }

        private MapLoadContextInstance[] GenerateBossMap()
        {
            var boss = RandomUtils.GetRandomUniqueItems(bossMaps, 1);
            var reward = GetRandomUniqueReward(out RewardType rewardType);
            return new [] {MapFactory.Create(boss[0], new RewardContext(reward ,rewardType))};
        }
        
        private  MapLoadContextInstance[] GenerateMapsForALevel()
        {
            //Each level has 2 options out of all the maps
            var maps = RandomUtils.GetRandomUniqueItems(monsterMaps, 2);
            Debug.Log("Generating maps for a level" + maps[0].name + " " + maps[1].name);
            //decide get unique reward type
            //random bool
            if (Random.value > 0.5f)
            {
                var map1 = MapFactory.Create(maps[0], new RewardContext(GetRandomEquipment().ToArray(), RewardType.Equipment));
                var map2 = MapFactory.Create(maps[1], new RewardContext(GetRandomSupply().ToArray(), RewardType.Supply));
                return new [] {map1, map2};
            }
            else
            {
                var map1 = MapFactory.Create(maps[0], new RewardContext(GetRandomSupply().ToArray(), RewardType.Supply));
                var map2 = MapFactory.Create(maps[1], new RewardContext(GetRandomEquipment().ToArray(), RewardType.Equipment));
                return new[] { map1, map2 };
            }
        }

        private List<ItemData> GetRandomEquipment()
        {
            return RandomUtils.GetRandomUniqueItems(_rewardDataBase.EquipmentRewards, 3);
        }

        private List<ItemData> GetRandomSupply()
        {
            return RandomUtils.GetRandomUniqueItems(_rewardDataBase.SupplyRewards, 3);
        }

        private ItemData[] GetRandomUniqueReward(out RewardType rewardType)
        {
            var reward = RandomUtils.GetRandomUniqueItems(_rewardDataBase.EquipmentRewards, 3);
            rewardType = RewardType.Equipment;
            return reward.ToArray();
        }
        
    }
}