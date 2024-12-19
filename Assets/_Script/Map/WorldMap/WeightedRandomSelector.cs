// Author : Peiyu Wang @ Daphatus
// 19 12 2024 12 08

using System.Collections.Generic;

namespace _Script.Map.WorldMap
{
    public static class WeightedRandomSelector
    {
        public static T PickRandomWeighted<T>(Dictionary<T, int> weights)
        {
            int totalWeight = 0;
            foreach (var w in weights.Values)
            {
                totalWeight += w;
            }

            int rand = UnityEngine.Random.Range(0, totalWeight);
            int cumulative = 0;
            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (rand < cumulative)
                {
                    return kvp.Key;
                }
            }

            // fallback (should never get here if weights sum > 0)
            throw new System.Exception("No weighted item selected, check weights.");

        }
    }
}