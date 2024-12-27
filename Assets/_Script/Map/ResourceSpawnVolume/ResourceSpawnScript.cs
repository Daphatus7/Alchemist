// Author : Peiyu Wang @ Daphatus
// 27 12 2024

using System.Collections.Generic;
using UnityEngine;

namespace _Script.Map.ResourceSpawnVolume
{
    [CreateAssetMenu(fileName = "ResourceSpawnScript", menuName = "GameData/ResourceSpawnScript")]
    public class ResourceSpawnScript : ScriptableObject, IResourceSpawnProvider
    {
        [System.Serializable]
        public class ResourceItem
        {
            [Tooltip("Prefab to spawn (e.g., a tree, rock, ore deposit, etc.)")]
            public GameObject resourcePrefab;

            [Range(0f, 100f), Tooltip("Weight or chance for this resource (0..1).")]
            public float spawnChance = 5f;
            
        }

        [SerializeField, Tooltip("All possible resource types that can spawn.")]
        private ResourceItem[] resourceItems;

        /// <summary>
        /// Return the list of possible resource items so the spawner can iterate over them.
        /// </summary>
        public IEnumerable<ResourceItem> GetResources()
        {
            return resourceItems;
        }
    }

    /// <summary>
    /// Interface so that ResourceSpawnVolume can fetch a list of resource items.
    /// </summary>
    public interface IResourceSpawnProvider
    {
        IEnumerable<ResourceSpawnScript.ResourceItem> GetResources();
    }
}