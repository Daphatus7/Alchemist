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
        [Range(0f, 5f)]
        [SerializeField]
        private float spawnDensity = 1f;

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
            float boxArea = _box2D.size.x * _box2D.size.y;
            var resourceList = new List<ResourceSpawnScript.ResourceItem>(_resourceProvider.GetResources());

            // For each resource, multiply its spawnChance by (area * density)
            // to get how many times we attempt to spawn that resource.
            foreach (var resource in resourceList)
            {
                if (resource.resourcePrefab == null || resource.spawnChance <= 0f)
                    continue;

                // Expected number of "spawn attempts" for this resource
                float expectedSpawns = resource.spawnChance * spawnDensity * boxArea / 100;
                int spawnCount = Mathf.RoundToInt(expectedSpawns);
                
                for(int i = 0; i < spawnCount; i++)
                {
                    Vector3 spawnPos = GetRandomPointInsideBox(_box2D);
                    GameObject resourceObj = Instantiate(resource.resourcePrefab, spawnPos, Quaternion.identity);
                    resourceObj.transform.parent = transform;
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