// Author : Peiyu Wang @ Daphatus
// 10 01 2025 01 31

using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.Map.Volume
{
    public class ResourceSpawnVolume : MonoBehaviour
    {
        [Header("Resource Data (ScriptableObject)")]
        [Tooltip("A ScriptableObject implementing IResourceSpawnProvider (e.g., ResourceSpawnScript).")]
        [SerializeField]
        private ScriptableObject resourceSpawnScript;

        [Header("Spawn Density")]
        [Tooltip("Spawns per unit area. For example, 1 => For an area=10, we expect about 10 * (resource.spawnChance) spawns of that resource.")]
        [Range(0f, 100f)]
        [SerializeField]
        private float spawnDensity = 2f;
        
        
        
        [Header("Monster Spawn Density")]
        
        [SerializeField]
        private ScriptableObject monsterSpawnScript;
        
        [Tooltip("0 -> no monster, 100 -> 1 monster every 1 unit area")]
        [Range(0f, 100f)]
        [SerializeField] private float monsterSpawnDensity = 1f; 
        
        
        private IResourceSpawnProvider _resourceProvider;

        [Button]
        public void Spawn(ReachableArea reachableArea)
        {
            _resourceProvider = resourceSpawnScript as IResourceSpawnProvider;
            StartCoroutine(SpawnResources(reachableArea));
        }
        
        private IEnumerator SpawnResources(ReachableArea reachableArea)
        {
            // Small delay to ensure things are ready
            yield return new WaitForSeconds(0.1f);

            // Calculate the area of the box
            // (simple approach assuming no rotation)
            // For each resource, multiply its spawnChance by (area * density)
            // to get how many times we attempt to spawn that resource.

            if (resourceSpawnScript)
            {
                List<GameObject> itemToSpawn = ((ResourceSpawnScript)resourceSpawnScript).GetResourceToSpawn(reachableArea.AreaSize * spawnDensity / 100);
                if (itemToSpawn.Count > 0)
                {
                    foreach (var resource in itemToSpawn)
                    {
                        if (!resource)
                            continue;

                        Vector3 spawnPos = GetRandomPointInsideBox(reachableArea);
                        GameObject resourceObj = Instantiate(resource, spawnPos, Quaternion.identity);
                        resourceObj.transform.parent = transform;
                    }
                }
            }
            
            if (monsterSpawnScript)
            {
                List<GameObject> monsterList = ((ResourceSpawnScript)monsterSpawnScript).GetResourceToSpawn(reachableArea.AreaSize * monsterSpawnDensity / 100);

                if (monsterList.Count <= 0) yield break;
                {
                    foreach (var monster in monsterList)
                    {
                        if (!monster)
                            continue;

                        Vector3 spawnPos = GetRandomPointInsideBox(reachableArea);
                        GameObject resourceObj = Instantiate(monster, spawnPos, Quaternion.identity);
                        resourceObj.transform.parent = transform;
                    }
                }
            }

        }

        /// <summary>
        /// Returns a random point inside the BoxCollider2D's area (ignoring rotation).
        /// </summary>
        private Vector3 GetRandomPointInsideBox(ReachableArea reachableArea)
        {
            
            var randomPosition = reachableArea.GetARandomPosition();
            return randomPosition;
        }
    }
}