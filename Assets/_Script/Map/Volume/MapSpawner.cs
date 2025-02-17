// Author : Peiyu Wang @ Daphatus
// 10 01 2025 01 31

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Enemy.EnemyCharacter;
using _Script.Enemy.EnemyData;
using _Script.Enemy.EnemyDatabase;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Items.Lootable;
using _Script.Managers;
using _Script.Map.WorldMap;
using _Script.Quest;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map.Volume
{
    public class MapSpawner : MonoBehaviour
    {
        [Header("Resource Data (ScriptableObject)")]
        [Tooltip("A ScriptableObject implementing IResourceSpawnProvider (e.g., ResourceSpawnScript).")]
        [SerializeField]
        private ScriptableObject resourceSpawnScript;

        [Header("Spawn Density")]
        [Tooltip("Spawns per unit area. For example, 1 => For an area=10, we expect about 10 * (resource.spawnChance) spawns of that resource.")]
        [Range(0f, 1f)]
        [SerializeField]
        private float spawnDensity = 0.5f;
        
        [Header("Monster Spawn Density")]
        [SerializeField]
        private ScriptableObject monsterSpawnScript;
        
        [Tooltip("0 -> no monster, 100 -> 1 monster every 1 unit area")]
        [Range(0f, 1f)]
        [SerializeField] 
        private float monsterSpawnDensity = 1f; 
        
        private IResourceSpawnProvider _resourceProvider;
        
        [Header("Boss Spawn Point")]
        [SerializeField] private Transform _spawnPoint;

        /// <summary>
        /// Spawns objects using a ReachableArea as the spawn boundary.
        /// </summary>
        public void Spawn(ReachableArea reachableArea, NodeDataInstance nodeDataInstance)
        {
            _resourceProvider = resourceSpawnScript as IResourceSpawnProvider;
            StartCoroutine(SpawnResources(reachableArea, nodeDataInstance));
        }

        /// <summary>
        /// Designed for special cases where the spawn point is fixed.
        /// </summary>
        public void Spawn( NodeDataInstance nodeDataInstance)
        {
            _resourceProvider = resourceSpawnScript as IResourceSpawnProvider;
            StartCoroutine(SpawnAtTransform(_spawnPoint, nodeDataInstance));
        }
        
        /// <summary>
        /// Coroutine to spawn resources and monsters within the bounds of a ReachableArea.
        /// </summary>
        private IEnumerator SpawnResources(ReachableArea reachableArea, NodeDataInstance nodeDataInstance)
        {
            // Small delay to ensure things are ready
            yield return new WaitForSeconds(0.1f);

            // Spawn Resources
            if (resourceSpawnScript)
            {
                // Calculate spawn count based on area size and density.
                float spawnCountFactor = reachableArea.AreaSize * spawnDensity / 100f;
                List<GameObject> itemsToSpawn = ((ResourceSpawnScript)resourceSpawnScript).GetResourceToSpawn(spawnCountFactor);
                foreach (var resource in itemsToSpawn)
                {
                    if (!resource)
                        continue;

                    Vector3 spawnPos = GetRandomPointInsideBox(reachableArea);
                    var resourceObj = Instantiate(resource, spawnPos, Quaternion.identity);
                    resourceObj.transform.parent = transform;
                }
            }
            
            // Spawn Monsters
            if (monsterSpawnScript)
            {
                float monsterSpawnFactor = reachableArea.AreaSize * monsterSpawnDensity / 100f;
                List<GameObject> monsterList = ((ResourceSpawnScript)monsterSpawnScript).GetResourceToSpawn(monsterSpawnFactor);
                foreach (var monster in monsterList)
                {
                    if (!monster)
                        continue;
                    
                    Vector3 spawnPos = GetRandomPointInsideBox(reachableArea);
                    var monsterObj = Instantiate(monster, spawnPos, Quaternion.identity);
                    var enemyCharacter = monsterObj.GetComponent<EnemyCharacter>();
                    enemyCharacter.Initialize(nodeDataInstance.MapRank);
                    monsterObj.transform.parent = transform;
                }
            }
            
            // Spawn Quest Object if applicable
            if (nodeDataInstance is QuestNodeInstance questNodeInstance)
            {
                Debug.Log("Spawning quest object.");
                switch (questNodeInstance.ObjectiveType)
                {
                    case ObjectiveType.Collect:
                        if (questNodeInstance is CollectNodeInstance collectNodeInstance)
                        {
                            var itemData = DatabaseManager.Instance.GetItemData(collectNodeInstance.ItemName);
                            if (itemData)
                            {
                                Vector3 spawnPos = GetRandomPointInsideBox(reachableArea);
                                var lootObj = ItemLootable.DropItem(spawnPos, itemData, 1);
                                lootObj.transform.parent = transform;
                            }
                        }   
                        break;
                    case ObjectiveType.Kill:
                        if (questNodeInstance is BossNodeInstance bossNodeInstance)
                        {
                            Debug.Log("Spawning boss object.");
                            var bossPrefab = DatabaseManager.Instance.GetEnemyPrefab(bossNodeInstance.BossName);
                            if (bossPrefab)
                            {
                                Debug.Log($"Spawning boss {bossNodeInstance.BossName}.");
                                Vector3 spawnPos = GetRandomPointInsideBox(reachableArea);
                                var bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
                                bossObj.transform.parent = transform;
                                var enemyCharacter = bossObj.GetComponent<EnemyCharacter>();
                                enemyCharacter.Initialize(nodeDataInstance.MapRank);
                            }
                            else
                            {
                                Debug.LogError($"Boss prefab {bossNodeInstance.BossName} not found in database.");
                            }
                        }
                        break;
                    case ObjectiveType.Explore:
                        throw new NotImplementedException("Explore Node not implemented yet");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Coroutine to spawn resources, monsters, and quest objects at a fixed Transform position.
        /// A small random offset is applied to each spawn position.
        /// </summary>
        private IEnumerator SpawnAtTransform(Transform spawnPoint, NodeDataInstance nodeDataInstance)
        {
            // Small delay to ensure everything is ready
            yield return new WaitForSeconds(0.1f);

            // For transform-based spawns, we use a fixed area factor (e.g., 1 unit area).
            float areaFactor = 1f;

            // Spawn Resources
            if (resourceSpawnScript)
            {
                List<GameObject> itemsToSpawn = ((ResourceSpawnScript)resourceSpawnScript).GetResourceToSpawn(areaFactor * spawnDensity);
                foreach (var resource in itemsToSpawn)
                {
                    if (!resource)
                        continue;

                    Vector3 spawnPos = spawnPoint.position + GetRandomOffset();
                    var resourceObj = Instantiate(resource, spawnPos, Quaternion.identity);
                    resourceObj.transform.parent = transform;
                }
            }

            // Spawn Monsters
            if (monsterSpawnScript)
            {
                List<GameObject> monsterList = ((ResourceSpawnScript)monsterSpawnScript).GetResourceToSpawn(areaFactor * monsterSpawnDensity);
                foreach (var monster in monsterList)
                {
                    if (!monster)
                        continue;

                    Vector3 spawnPos = spawnPoint.position + GetRandomOffset();
                    var monsterObj = Instantiate(monster, spawnPos, Quaternion.identity);
                    var enemyCharacter = monsterObj.GetComponent<EnemyCharacter>();
                    enemyCharacter.Initialize(nodeDataInstance.MapRank);
                    monsterObj.transform.parent = transform;
                }
            }

            // Spawn Quest Object if applicable
            if (nodeDataInstance is QuestNodeInstance questNodeInstance)
            {
                switch (questNodeInstance.ObjectiveType)
                {
                    case ObjectiveType.Collect:
                        if (questNodeInstance is CollectNodeInstance collectNodeInstance)
                        {
                            var itemData = DatabaseManager.Instance.GetItemData(collectNodeInstance.ItemName);
                            if (itemData)
                            {
                                Vector3 spawnPos = spawnPoint.position + GetRandomOffset();
                                var lootObj = ItemLootable.DropItem(spawnPos, itemData, 1);
                                lootObj.transform.parent = transform;
                            }
                        }   
                        break;
                    case ObjectiveType.Kill:
                        if (questNodeInstance is BossNodeInstance bossNodeInstance)
                        {
                            var bossPrefab = DatabaseManager.Instance.GetEnemyPrefab(bossNodeInstance.BossName);
                            if (bossPrefab)
                            {
                                Vector3 spawnPos = spawnPoint.position + GetRandomOffset();
                                var bossObj = Instantiate(bossPrefab, spawnPos, Quaternion.identity);
                                bossObj.transform.parent = transform;
                                var enemyCharacter = bossObj.GetComponent<EnemyCharacter>();
                                enemyCharacter.Initialize(nodeDataInstance.MapRank);
                            }
                            else
                            {
                                Debug.LogError($"Boss prefab {bossNodeInstance.BossName} not found in database.");
                            }
                        }
                        break;
                    case ObjectiveType.Explore:
                        throw new NotImplementedException("Explore Node not implemented yet");
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Returns a random point inside the given ReachableArea's bounds.
        /// </summary>
        private Vector3 GetRandomPointInsideBox(ReachableArea reachableArea)
        {
            return reachableArea.GetARandomPosition();
        }

        /// <summary>
        /// Returns a small random offset on the X and Z axes.
        /// </summary>
        private Vector3 GetRandomOffset()
        {
            // Adjust the offset range as necessary.
            float offsetX = UnityEngine.Random.Range(-1f, 1f);
            float offsetZ = UnityEngine.Random.Range(-1f, 1f);
            return new Vector3(offsetX, 0, offsetZ);
        }
    }
}