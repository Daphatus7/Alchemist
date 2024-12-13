// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 22

using System.Collections.Generic;
using UnityEngine;

namespace _Script.Enemy.DropTable
{

    [CreateAssetMenu(fileName = "DropTable", menuName = "GameData/DropTable")]
    public class DropTable : ScriptableObject, IDropProvider 
    {
        [System.Serializable]
        public class DropItem
        {
            public GameObject itemPrefab; // Prefab of the item to drop
            public float dropChance;      // Probability of this item dropping (e.g., 0.2 for 20%)
            public int minAmount;         // Minimum quantity if dropped
            public int maxAmount;         // Maximum quantity if dropped
            public bool isUnique;         // If true, this item can only drop once globally or per instance
        }

        public DropItem[] drops; // Array of possible drops for this table
        
        public IEnumerable<DropItem> GetDrops()
        {
            return drops;
        }
    }
}