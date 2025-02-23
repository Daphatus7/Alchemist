// Author : Peiyu Wang @ Daphatus
// 27 12 2024

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Script.Map.Volume
{
    [CreateAssetMenu(fileName = "ResourceSpawnScript", menuName = "GameData/ResourceSpawnScript")]
    public class ResourceSpawnScript : ScriptableObject, IResourceSpawnProvider
    {
        [System.Serializable]
        public class ResourceItem
        {
            [Tooltip("Prefab to spawn (e.g., a tree, rock, ore deposit, etc.)")]
            public GameObject resourcePrefab;

            [Tooltip("More weight the higher the chance of spawning.")]
            public int weight = 5;
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
        
        //get weight sum
        public int GetWeightSum()
        {
            int sum = 0;
            foreach (var item in resourceItems)
            {
                sum += item.weight;
            }

            return sum;
        }
        
        //get random resource
        public List<GameObject> GetResourceToSpawn(float rate)
        {
            var result = new List<GameObject>();
            if (rate <= 0) return result;

            // Put all resources into a weighted list
            var weightedList = new int[resourceItems.Count()];
            for (int i = 0; i < resourceItems.Count(); i++)
            {
                weightedList[i] = resourceItems[i].weight;
            }
            // Calculate the integer and fractional parts of the rate
            int integerPart = Mathf.FloorToInt(rate); // Integer part of the rate
            float fractionalPart = rate - integerPart; // Fractional part of the rate

            // Determine if an extra unit should be spawned based on the fractional part
            int count = integerPart + (Random.value < fractionalPart ? 1 : 0);


            // Total weight of all resources
            var totalWeight = GetWeightSum();

            // Spawn resources based on the calculated count
            for (var i = 0; i < count; i++)
            {
                var random = Random.Range(0, totalWeight);
                var sum = 0;
                for (int j = 0; j < resourceItems.Count(); j++)
                {
                    sum += weightedList[j];
                    if (random < sum)
                    {
                        result.Add(resourceItems[j].resourcePrefab);
                        break;
                    }
                }
            }

            return result;
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