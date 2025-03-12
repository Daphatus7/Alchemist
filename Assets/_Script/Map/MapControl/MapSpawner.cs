// Author : Peiyu Wang @ Daphatus
// 10 01 2025 01 31

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Enemy.EnemyCharacter;
using _Script.Items.Lootable;
using _Script.Managers;
using _Script.Map.MapLoadContext;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.Volume;
using UnityEngine;

namespace _Script.Map
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
        
        
        
        [Header("Boss Spawn Point")]
        [SerializeField] 
        private Transform _spawnPoint;

        private IResourceSpawnProvider _resourceProvider;
        
        /// <summary>
        /// Spawns objects using a ReachableArea as the spawn boundary.
        /// </summary>
        public void Spawn(ReachableArea reachableArea, MapLoadContextInstance instance)
        {
            _resourceProvider = resourceSpawnScript as IResourceSpawnProvider;
            StartCoroutine(SpawnResources(reachableArea, instance));
        }

        /// <summary>
        /// Designed for special cases where the spawn point is fixed.
        /// </summary>
        public void Spawn(MapLoadContextInstance instance)
        {
            _resourceProvider = resourceSpawnScript as IResourceSpawnProvider;
            StartCoroutine(SpawnAtTransform(_spawnPoint, instance));
        }
        
        /// <summary>
        /// Coroutine to spawn resources, monsters, and quest objects within the bounds of a ReachableArea.
        /// </summary>
        private IEnumerator SpawnResources(ReachableArea reachableArea, MapLoadContextInstance instance)
        {
            yield return new WaitForSeconds(0.1f);
            
            // Define spawn position using the reachable area's bounds.
            Func<Vector3> getSpawnPos = () => GetRandomPointInsideBox(reachableArea);
            float resourceFactor = reachableArea.AreaSize * spawnDensity / 100f;
            float monsterFactor = reachableArea.AreaSize * monsterSpawnDensity / 100f;
            
            SpawnCategory(resourceSpawnScript, resourceFactor, getSpawnPos);
            if(instance != null)
            {
                SpawnCategory(monsterSpawnScript, monsterFactor, getSpawnPos, (go) =>
                {
                    var enemyCharacter = go.GetComponent<EnemyCharacter>();
                    
                    //Where enemyCharacter is an instance of EnemyCharacter
                    enemyCharacter.Initialize(instance.MapRank);
                });
            }
        }

        /// <summary>
        /// Coroutine to spawn resources, monsters, and quest objects at a fixed Transform position.
        /// </summary>
        private IEnumerator SpawnAtTransform(Transform spawnPoint, MapLoadContextInstance instance)
        {
            yield return new WaitForSeconds(0.1f);
            
            // For fixed spawns, we use a default area factor (e.g., 1 unit).
            float areaFactor = 1f;
            Func<Vector3> getSpawnPos = () => spawnPoint.position + GetRandomOffset();
            float resourceFactor = areaFactor * spawnDensity;
            float monsterFactor = areaFactor * monsterSpawnDensity;
            
            SpawnCategory(resourceSpawnScript, resourceFactor, getSpawnPos);
            SpawnCategory(monsterSpawnScript, monsterFactor, getSpawnPos, (go) => {
                var enemyCharacter = go.GetComponent<EnemyCharacter>();
                enemyCharacter.Initialize(instance.MapRank);
            });
        }
        
        /// <summary>
        /// Helper method to spawn a category of objects (resources or monsters).
        /// </summary>
        /// <param name="spawnScript">ScriptableObject that provides spawnable objects.</param>
        /// <param name="factor">Spawn count factor.</param>
        /// <param name="getSpawnPos">Function to compute a spawn position.</param>
        /// <param name="postSpawnAction">Optional action to execute on each spawned object.</param>
        private void SpawnCategory(ScriptableObject spawnScript, 
            float factor, Func<Vector3> getSpawnPos, 
            Action<GameObject> postSpawnAction = null)
        {
            if (spawnScript == null) return;
            
            List<GameObject> items = ((ResourceSpawnScript)spawnScript).GetResourceToSpawn(factor);
            foreach (var item in items)
            {
                if (item == null) continue;
                Vector3 pos = getSpawnPos();
                var obj = Instantiate(item, pos, Quaternion.identity);
                obj.transform.parent = transform;
                postSpawnAction?.Invoke(obj);
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
            float offsetX = UnityEngine.Random.Range(-1f, 1f);
            float offsetZ = UnityEngine.Random.Range(-1f, 1f);
            return new Vector3(offsetX, 0, offsetZ);
        }
    }
}