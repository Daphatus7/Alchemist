// Author : Peiyu Wang @ Daphatus
// 17 02 2025 02 05

using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Managers.Database
{
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Managers.Database
{
    /// <summary>
    /// Runtime wrapper for the ItemDatabase asset.
    /// Builds a lookup dictionary so that item data can be accessed efficiently during gameplay.
    /// </summary>
    public class ItemDatabaseRuntime
    {
        private readonly Dictionary<string, ItemData> _itemDictionary;
        private readonly ItemDatabase _itemDatabaseAsset;

        /// <summary>
        /// Initializes the runtime item database from the provided asset.
        /// </summary>
        /// <param name="databaseAsset">The ItemDatabase asset reference.</param>
        public ItemDatabaseRuntime(ItemDatabase databaseAsset)
        {
            if (databaseAsset == null)
            {
                Debug.LogError("ItemDatabaseRuntime: Provided ItemDatabase asset is null.");
                _itemDictionary = new Dictionary<string, ItemData>();
                return;
            }

            _itemDatabaseAsset = databaseAsset;
            _itemDictionary = BuildDictionary();
        }

        /// <summary>
        /// Builds the dictionary from the Items list in the ItemDatabase asset.
        /// </summary>
        /// <returns>A dictionary mapping item IDs to ItemData.</returns>
        private Dictionary<string, ItemData> BuildDictionary()
        {
            var dict = new Dictionary<string, ItemData>();

            foreach (var wrapped in _itemDatabaseAsset.Items)
            {
                if (wrapped.itemData != null && !string.IsNullOrEmpty(wrapped.itemData.ItemID))
                {
                    if (!dict.ContainsKey(wrapped.itemData.ItemID))
                    {
                        dict.Add(wrapped.itemData.ItemID, wrapped.itemData);
                    }
                    else
                    {
                        Debug.LogWarning($"ItemDatabaseRuntime: Duplicate item ID detected: {wrapped.itemData.ItemID}");
                    }
                }
            }

            return dict;
        }

        /// <summary>
        /// Retrieves the ItemData associated with the given item ID.
        /// </summary>
        /// <param name="itemID">The item ID key.</param>
        /// <returns>The ItemData if found; otherwise, null.</returns>
        public ItemData GetItemData(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogError("ItemDatabaseRuntime: Provided item ID is null or empty.");
                return null;
            }

            if (_itemDictionary.TryGetValue(itemID, out ItemData itemData))
            {
                return itemData;
            }

            Debug.LogError($"ItemDatabaseRuntime: No item found with ID '{itemID}'.");
            return null;
        }
    }
}
}