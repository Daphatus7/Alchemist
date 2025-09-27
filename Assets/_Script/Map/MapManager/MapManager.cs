// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 10

using System;
using System.Collections.Generic;
using System.Linq;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Managers;
using _Script.Map.MapLoadContext;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.MapLoadContext.RewardContext;
using _Script.Map.MapLoadContext.Scriptable;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


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


        /// <summary>
        /// The level is finished, all monsters are defeated etc.
        /// </summary>
        public bool IsLevelCompleted
        {
            get
            {
                return _currentMap != null && _currentMap.IsCompleted;
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

        [SerializeField] private BossMapLoadContext [] finalBossMaps;
        
        [SerializeField] private MonsterMapLoadContext [] monsterMaps;

        [SerializeField] private TownMapLoadContext townMap;
        
        [SerializeField, Min(1)] [FormerlySerializedAs("minimumMiniMapCount")] private int minimumMiniMapsPerLevel = 2;
        [SerializeField, Min(1)] [FormerlySerializedAs("maximumMiniMapCount")] [FormerlySerializedAs("miniMapCount")] private int maximumMiniMapsPerLevel = 3;
        [SerializeField, Min(1)] private int minimumLevelsBeforeFinalBoss = 3;
        [SerializeField, Min(1)] private int maximumLevelsBeforeFinalBoss = 4;
        [SerializeField, Min(1)] private int minimumGateCount = 2;
        [SerializeField, Min(1)] private int maximumGateCount = 3;
            
        [SerializeField] private RewardDataBase _rewardDataBase;
        
        /// <summary>
        /// Generate the game maps
        /// </summary>
        public void InitializeMaps()
        {
            _allMaps.Clear();

            if (townMap == null)
            {
                throw new System.Exception("Town map is null");
            }

            if (_rewardDataBase == null)
            {
                throw new System.Exception("Reward database is null");
            }

            //consider the town map
            var town = MapFactory.Create(townMap, CreateRewardContext(RewardType.Equipment));
            _allMaps.Enqueue(new [] {town});
            CurrentMap = town;
            GenerateGameMaps();
        }

        /// <summary>
        /// Called before current map is completed and the player is entering the next map
        /// </summary>
        public void EnterMap(MapLoadContextInstance map)
        {
            //load the new map
            if (_allMaps.Count == 0)
            {
                Debug.LogWarning("No maps queued. Did you reach the end of the run?");
                return;
            }

            var currentTier = _allMaps.Dequeue();
            if (currentTier != null && !currentTier.Contains(map))
            {
                Debug.LogWarning("Selected map was not part of the current tier.");
            }
            CurrentMap = map;
            GameManager.Instance.LoadSelectedScene(map);
            Debug.Log("Loading map " + map.MapName + " " + map.MapRank);
            //load the new map
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
            var levelCount = GetLevelCountBeforeFinalBoss();
            for (var levelIndex = 0; levelIndex < levelCount; levelIndex++)
            {
                var miniMapIterations = GetMiniMapCountForLevel();
                for (var miniMapIndex = 0; miniMapIndex < miniMapIterations; miniMapIndex++)
                {
                    var maps = GenerateMiniMapOptions();
                    if (maps == null || maps.Length == 0)
                    {
                        throw new System.Exception("Maps are null");
                    }

                    _allMaps.Enqueue(maps);
                }

                var bossTier = GenerateBossMap(levelIndex == levelCount - 1);
                if (bossTier == null || bossTier.Length == 0)
                {
                    throw new System.Exception("Boss is null");
                }

                _allMaps.Enqueue(bossTier);
            }
        }

        private MapLoadContextInstance[] GenerateBossMap(bool useFinalBoss)
        {
            var candidates = GetBossCandidates(useFinalBoss);
            var boss = RandomUtils.GetRandomUniqueItems(candidates, 1);
            if (boss == null || boss.Count == 0)
            {
                throw new System.Exception("Boss maps are null");
            }

            var rewardContext = CreateRewardContext(RewardType.Boss);
            return new [] {MapFactory.Create(boss[0], rewardContext)};
        }

        private IEnumerable<BossMapLoadContext> GetBossCandidates(bool useFinalBoss)
        {
            if (useFinalBoss && finalBossMaps != null && finalBossMaps.Length > 0)
            {
                return finalBossMaps;
            }

            if (bossMaps == null || bossMaps.Length == 0)
            {
                throw new System.Exception(useFinalBoss ? "Final boss maps are null" : "Boss maps are null");
            }

            return bossMaps;
        }

        private  MapLoadContextInstance[] GenerateMiniMapOptions()
        {
            var gateCount = GetGateCountForMiniMap();
            var maps = RandomUtils.GetRandomUniqueItems(monsterMaps, gateCount);
            if (maps == null || maps.Count == 0)
            {
                throw new System.Exception("Maps are null");
            }

            Debug.Log("Generating maps for a level with " + maps.Count + " gates.");

            var rewardPattern = BuildRewardPattern(maps.Count);
            var contexts = new MapLoadContextInstance[maps.Count];

            for (var i = 0; i < maps.Count; i++)
            {
                var rewardType = rewardPattern[i];
                var rewardContext = CreateRewardContext(rewardType);
                contexts[i] = MapFactory.Create(maps[i], rewardContext);
            }

            return contexts;
        }

        private List<ItemData> GetRandomEquipment()
        {
            var rewards = RandomUtils.GetRandomUniqueItems(_rewardDataBase.EquipmentRewards, 3);
            if (rewards == null || rewards.Count == 0)
            {
                throw new System.Exception("Equipment rewards are null");
            }

            return rewards;
        }

        private List<ItemData> GetRandomSupply()
        {
            var rewards = RandomUtils.GetRandomUniqueItems(_rewardDataBase.SupplyRewards, 3);
            if (rewards == null || rewards.Count == 0)
            {
                throw new System.Exception("Supply rewards are null");
            }

            return rewards;
        }

        private RewardContext CreateRewardContext(RewardType rewardType)
        {
            switch (rewardType)
            {
                case RewardType.Equipment:
                    return new RewardContext(GetRandomEquipment().ToArray(), RewardType.Equipment);
                case RewardType.Supply:
                    return new RewardContext(GetRandomSupply().ToArray(), RewardType.Supply);
                case RewardType.Boss:
                    return new RewardContext(Array.Empty<ItemData>(), RewardType.Boss);
                default:
                    throw new System.ArgumentOutOfRangeException(nameof(rewardType), rewardType, null);
            }
        }

        private RewardType[] BuildRewardPattern(int gateCount)
        {
            if (gateCount <= 0)
            {
                return System.Array.Empty<RewardType>();
            }

            var rewardTypes = new List<RewardType>(gateCount);
            if (gateCount == 1)
            {
                rewardTypes.Add(RewardType.Equipment);
            }
            else
            {
                rewardTypes.Add(RewardType.Equipment);
                rewardTypes.Add(RewardType.Supply);
                for (var i = rewardTypes.Count; i < gateCount; i++)
                {
                    rewardTypes.Add(Random.value > 0.5f ? RewardType.Equipment : RewardType.Supply);
                }
            }

            return rewardTypes.OrderBy(_ => Random.value).ToArray();
        }

        private int GetMiniMapCountForLevel()
        {
            var minValue = Mathf.Max(1, Mathf.Min(minimumMiniMapsPerLevel, maximumMiniMapsPerLevel));
            var maxValue = Mathf.Max(minValue, Mathf.Max(minimumMiniMapsPerLevel, maximumMiniMapsPerLevel));
            return Random.Range(minValue, maxValue + 1);
        }

        private int GetLevelCountBeforeFinalBoss()
        {
            var minValue = Mathf.Max(1, Mathf.Min(minimumLevelsBeforeFinalBoss, maximumLevelsBeforeFinalBoss));
            var maxValue = Mathf.Max(minValue, Mathf.Max(minimumLevelsBeforeFinalBoss, maximumLevelsBeforeFinalBoss));
            return Random.Range(minValue, maxValue + 1);
        }

        private int GetGateCountForMiniMap()
        {
            if (monsterMaps == null || monsterMaps.Length == 0)
            {
                throw new System.Exception("Monster maps are null");
            }

            var minValue = Mathf.Max(1, Mathf.Min(minimumGateCount, maximumGateCount));
            var maxValue = Mathf.Max(minValue, Mathf.Max(minimumGateCount, maximumGateCount));

            var clampedMax = Mathf.Min(monsterMaps.Length, maxValue);
            var clampedMin = Mathf.Min(clampedMax, minValue);

            return Random.Range(clampedMin, clampedMax + 1);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            minimumMiniMapsPerLevel = Mathf.Max(1, minimumMiniMapsPerLevel);
            maximumMiniMapsPerLevel = Mathf.Max(minimumMiniMapsPerLevel, maximumMiniMapsPerLevel);

            minimumLevelsBeforeFinalBoss = Mathf.Max(1, minimumLevelsBeforeFinalBoss);
            maximumLevelsBeforeFinalBoss = Mathf.Max(minimumLevelsBeforeFinalBoss, maximumLevelsBeforeFinalBoss);

            minimumGateCount = Mathf.Max(1, minimumGateCount);
            maximumGateCount = Mathf.Max(minimumGateCount, maximumGateCount);
        }
#endif
        
    }
}
