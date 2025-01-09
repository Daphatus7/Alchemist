using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.Map.ResourceSpawnVolume
{
    [RequireComponent(typeof(BoxCollider2D))]
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
        
        
        private float BoxArea
        {
            get
            {
                if (_box2D == null)
                {
                    _box2D = GetComponent<BoxCollider2D>();
                }
                return _box2D.size.x * _box2D.size.y;
            }
        }
        
        
        [Header("Monster Spawn Density")]
        
        [SerializeField]
        private ScriptableObject monsterSpawnScript;
        
        [Tooltip("0 -> no monster, 100 -> 1 monster every 1 unit area")]
        [Range(0f, 100f)]
        [SerializeField] private float monsterSpawnDensity = 1f; 
        
        
        private IResourceSpawnProvider _resourceProvider;
        private BoxCollider2D _box2D;

        private void Start()
        {
            Spawn();
        }

        [Button]
        public void Spawn()
        {
            _box2D = GetComponent<BoxCollider2D>();
            _resourceProvider = resourceSpawnScript as IResourceSpawnProvider;
            StartCoroutine(SpawnResources());

        }
        

        private IEnumerator SpawnResources()
        {
            // Small delay to ensure things are ready
            yield return new WaitForSeconds(0.1f);

            // Calculate the area of the box
            // (simple approach assuming no rotation)
            // For each resource, multiply its spawnChance by (area * density)
            // to get how many times we attempt to spawn that resource.

            if (resourceSpawnScript)
            {
                List<GameObject> itemToSpawn = ((ResourceSpawnScript)resourceSpawnScript).GetResourceToSpawn(BoxArea * spawnDensity / 100);
                if (itemToSpawn.Count > 0)
                {
                    foreach (var resource in itemToSpawn)
                    {
                        if (!resource)
                            continue;

                        Vector3 spawnPos = GetRandomPointInsideBox(_box2D);
                        GameObject resourceObj = Instantiate(resource, spawnPos, Quaternion.identity);
                        resourceObj.transform.parent = transform;
                    }
                }
            }
            
            if (monsterSpawnScript)
            {
                List<GameObject> monsterList = ((ResourceSpawnScript)monsterSpawnScript).GetResourceToSpawn(BoxArea * monsterSpawnDensity / 100);

                if (monsterList.Count <= 0) yield break;
                {
                    foreach (var monster in monsterList)
                    {
                        if (!monster)
                            continue;

                        Vector3 spawnPos = GetRandomPointInsideBox(_box2D);
                        GameObject resourceObj = Instantiate(monster, spawnPos, Quaternion.identity);
                        resourceObj.transform.parent = transform;
                    }
                }
            }

        }

        /// <summary>
        /// Returns a random point inside the BoxCollider2D's area (ignoring rotation).
        /// </summary>
        private Vector3 GetRandomPointInsideBox(BoxCollider2D box)
        {
            Vector2 boxSize = box.size;
            Vector2 boxOffset = box.offset;
            Vector2 boxCenter = (Vector2)box.transform.position + boxOffset;

            float randomX = Random.Range(boxCenter.x - boxSize.x / 2f,
                                         boxCenter.x + boxSize.x / 2f);
            float randomY = Random.Range(boxCenter.y - boxSize.y / 2f,
                                         boxCenter.y + boxSize.y / 2f);

            return new Vector3(randomX, randomY, 0f);
        }
    }
}