// Author : Peiyu Wang @ Daphatus
// Updated: [Current Date]

using System.Collections.Generic;
using System.Linq;
using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace _Script.Managers.Database
{
    /// <summary>
    /// ItemDatabase manages references to a collection of ItemData objects within the Inspector.
    /// It also provides a one-click option to export the database contents to a JSON file.
    /// </summary>
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Database/ItemDatabase")]
    public class ItemDatabase : SerializedScriptableObject
    {
        [ShowInInspector, DictionaryDrawerSettings(KeyLabel = "ItemType", ValueLabel = "NextIndex")]
        [InfoBox("How many IDs have been assigned per ItemType so far. New items of that type will start from the next index.")]
        public Dictionary<ItemType, int> ItemTypeCounters = new Dictionary<ItemType, int>()
        {
            { ItemType.Equipment, 0 },
            { ItemType.Consumable, 0 },
            { ItemType.Material, 0 },
            { ItemType.Seed, 0 },
            { ItemType.Fruit, 0 },
            { ItemType.Torch, 0 },
            { ItemType.Container, 0 },
        };

        //==================================
        // Custom mapping for ItemType abbreviations
        // (Not used anymore for modifying IDs)
        //==================================
        private static readonly Dictionary<ItemType, string> ItemTypeAbbreviations = new Dictionary<ItemType, string>()
        {
            { ItemType.Equipment, "EQ" },
            { ItemType.Consumable, "C" },
            { ItemType.Material, "M" },
            { ItemType.Seed, "S" },
            { ItemType.Fruit, "F" },
            { ItemType.Torch, "TO" },
            { ItemType.Container, "CT" },
        };
        
        //==================================
        // Data
        //==================================

        /// <summary>List of Items in this database. Each item is wrapped for inline editing.</summary>
        [BoxGroup("Current Database Content"), LabelText("Items in Database")]
        [TableList(AlwaysExpanded = true)]
        [InfoBox("You can directly edit each ItemData in this table. Changes apply to the corresponding .asset file.", InfoMessageType.Info)]
        public List<WrappedItem> Items = new List<WrappedItem>();

        /// <summary>Temporary list of items to add to the database (direct references). This converts to WrappedItem upon import.</summary>
        [BoxGroup("Current Database Content"), LabelText("Items to Add Temporarily")]
        [InfoBox("Select (multi-select) ItemData here, then click the button below to add them to the database.")]
        public List<ItemData> ItemsToAdd = new List<ItemData>();

        //==================================
        // Items not yet in the database
        //==================================
        [FoldoutGroup("Not in Database"), LabelText("ItemData not yet in this database"), PropertyOrder(10)]
        [TableList(IsReadOnly = true, AlwaysExpanded = true)]
        [InfoBox("Scans all ItemData in the project. Items not in this database are shown here. Click “Refresh List” to update.", InfoMessageType.Info)]
        public List<ItemData> NotInDatabase = new List<ItemData>();

        //==================================
        // Apply changes & Export
        //==================================

        [FoldoutGroup("Save"), PropertyOrder(-999)]
        [Button("Apply All Changes to Assets", ButtonSizes.Large), GUIColor(0.9f, 1f, 0.6f)]
        [InfoBox("Clicking this will mark both the database and all associated ItemData as 'dirty' and save them to disk.", InfoMessageType.None)]
        public void ApplyAllChanges()
        {
#if UNITY_EDITOR
            // Mark this database as dirty
            EditorUtility.SetDirty(this);

            // Mark each referenced ItemData as dirty
            foreach (var wrapped in Items)
            {
                if (wrapped != null && wrapped.itemData != null)
                {
                    EditorUtility.SetDirty(wrapped.itemData);
                }
            }

            // Save to disk
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[ItemDatabase] All changes to this database and its ItemData have been saved to disk.");
#else
            Debug.LogWarning("This operation is only available in the Unity Editor environment.");
#endif
        }

        /// <summary>
        /// One-click export of the database to a JSON file
        /// </summary>
        [FoldoutGroup("Save"), PropertyOrder(-998)]
        [Button("Export to JSON", ButtonSizes.Medium), GUIColor(0.9f, 1f, 0.8f)]
        [InfoBox("Export all item information in this database to a JSON file.")]
        public void ExportToJson()
        {
#if UNITY_EDITOR
            // 1. Ask the user where to save
            var path = EditorUtility.SaveFilePanel(
                "Export ItemDatabase to JSON",
                "Assets",
                "ItemDatabaseExport.json",
                "json"
            );

            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("Export canceled or no path specified.");
                return;
            }

            // 2. Prepare the data to export
            ItemDatabaseExport exportData = new ItemDatabaseExport();
            exportData.items = new List<ItemExportData>();

            foreach (var wrapped in Items)
            {
                if (wrapped.itemData == null) continue;

                var item = wrapped.itemData;
                var exportItem = new ItemExportData()
                {
                    itemID = item.ItemID,
                    itemName = item.ItemName,
                    itemDescription = item.ItemDescription,
                    itemType = item.ItemType.ToString(),
                    maxStackSize = item.MaxStackSize,
                };

                exportData.items.Add(exportItem);
            }

            // 3. Convert to JSON
            string json = JsonUtility.ToJson(exportData, prettyPrint: true);

            // 4. Write the file
            System.IO.File.WriteAllText(path, json, System.Text.Encoding.UTF8);

            // 5. Refresh the project (optional)
            AssetDatabase.Refresh();

            Debug.Log($"[ItemDatabase] Successfully exported JSON to: {path}\nContents:\n{json}");
#else
            Debug.LogWarning("JSON export is only available in the Unity Editor environment.");
#endif
        }

        //==================================
        // Batch operations: adding existing items
        //==================================

        [FoldoutGroup("Add & Remove"), LabelText("Batch-add existing items"), PropertyOrder(1)]
        [Button("Add Selected Items to Database"), GUIColor(0.4f, 1f, 0.4f)]
        [InfoBox("Imports all items from 'ItemsToAdd' into the 'Items' list (without modifying ItemID).")]
        public void AddSelectedItemsToDatabase()
        {
            int addedCount = 0;
            foreach (var item in ItemsToAdd)
            {
                if (item != null && !IsAlreadyInDatabase(item))
                {
                    // Do not modify the item's ID; simply wrap and add.
                    Items.Add(new WrappedItem { itemData = item });
                    addedCount++;
                }
            }
            ItemsToAdd.Clear();

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] Imported {addedCount} ItemData into the database (ItemID unchanged).");
        }

        /// <summary>
        /// One-click to add all items from the “NotInDatabase” list
        /// </summary>
        [FoldoutGroup("Not in Database"), PropertyOrder(11)]
        [Button("Add All 'NotInDatabase' Items"), GUIColor(0.4f, 1f, 0.4f)]
        public void AddAllNotInDatabase()
        {
            if (NotInDatabase == null || NotInDatabase.Count == 0)
            {
                Debug.LogWarning("[ItemDatabase] 'NotInDatabase' list is empty. Try clicking 'Refresh List' first.");
                return;
            }

            int addedCount = 0;
            foreach (var item in NotInDatabase)
            {
                if (item != null && !IsAlreadyInDatabase(item))
                {
                    // Do not modify the item's ID.
                    Items.Add(new WrappedItem { itemData = item });
                    addedCount++;
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] Imported {addedCount} items from 'NotInDatabase' (ItemID unchanged).");
        }

        //==================================
        // Refresh “NotInDatabase” list
        //==================================
        [FoldoutGroup("Not in Database"), PropertyOrder(10)]
        [Button("Refresh 'NotInDatabase' List"), GUIColor(1f, 0.9f, 0.4f)]
        public void RefreshNotInDatabase()
        {
#if UNITY_EDITOR
            NotInDatabase.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            int totalFound = 0;
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var itemData = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (itemData == null) continue;

                totalFound++;
                if (!IsAlreadyInDatabase(itemData))
                {
                    NotInDatabase.Add(itemData);
                }
            }

            Debug.Log($"[ItemDatabase] Scanned the project: found {totalFound} ItemData, of which {NotInDatabase.Count} are not in this database.");
#else
            Debug.LogWarning("This operation is only available in the Unity Editor environment.");
#endif
        }

        //==================================
        // Removal operations: soft-delete from Database list
        //==================================
        [FoldoutGroup("Add & Remove"), LabelText("Batch remove (by ID)"), PropertyOrder(2)]
        [Button("Remove Items by ID"), GUIColor(1f, 0.5f, 0.5f)]
        [InfoBox("Comma-separated multiple ItemIDs, e.g. 101,102,103. Only removes them from this database list, no physical asset deletion.")]
        public void RemoveItemsByIDs(string commaSeparatedIDs)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedIDs))
            {
                Debug.LogWarning("Please enter the comma-separated ItemID list!");
                return;
            }

            string[] idArray = commaSeparatedIDs.Split(',').Select(s => s.Trim()).ToArray();

            int removeCount = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var data = Items[i].itemData;
                if (data != null && idArray.Contains(data.ItemID))
                {
                    removeCount++;
                    Items.RemoveAt(i);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] Removed {removeCount} items matching ID (soft-delete).");
        }

        [FoldoutGroup("Add & Remove"), LabelText("Batch remove (by Name)"), PropertyOrder(3)]
        [Button("Remove Items by Name"), GUIColor(1f, 0.5f, 0.5f)]
        [InfoBox("Comma-separated multiple item Names, e.g. Apple,Axe. Only removes them from the database list, no physical asset deletion.")]
        public void RemoveItemsByNames(string commaSeparatedNames)
        {
            if (string.IsNullOrWhiteSpace(commaSeparatedNames))
            {
                Debug.LogWarning("Please enter the comma-separated item Name list!");
                return;
            }

            string[] nameArray = commaSeparatedNames.Split(',').Select(s => s.Trim()).ToArray();

            int removeCount = 0;
            for (int i = Items.Count - 1; i >= 0; i--)
            {
                var data = Items[i].itemData;
                if (data != null && nameArray.Contains(data.ItemName))
                {
                    removeCount++;
                    Items.RemoveAt(i);
                }
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif

            Debug.Log($"[ItemDatabase] Removed {removeCount} items matching Name (soft-delete).");
        }

        //==================================
        // Search
        //==================================
        [FoldoutGroup("Search"), LabelText("Search Item (by ID)"), PropertyOrder(4)]
        [Button("Search Item by ID"), GUIColor(0.7f, 0.7f, 1f)]
        public ItemData SearchItemByID(string itemID)
        {
            var wrapped = Items.FirstOrDefault(x => x.itemData != null && x.itemData.ItemID == itemID);
            if (wrapped == null || wrapped.itemData == null)
            {
                Debug.LogWarning($"[ItemDatabase] No item found with ID = {itemID}.");
                return null;
            }
            else
            {
                Debug.Log($"[ItemDatabase] Found item: {wrapped.itemData.ItemName} (ID = {wrapped.itemData.ItemID}).");
                return wrapped.itemData;
            }
        }

        [FoldoutGroup("Search"), LabelText("Search Item (by keyword in Name)"), PropertyOrder(5)]
        [Button("Search Item by Name"), GUIColor(0.7f, 0.7f, 1f)]
        public List<ItemData> SearchItemsByNameKeyword(string keyword)
        {
            if (string.IsNullOrEmpty(keyword))
            {
                Debug.LogWarning("[ItemDatabase] The keyword cannot be empty.");
                return null;
            }

            var matched = Items
                .Where(x => x.itemData != null &&
                            !string.IsNullOrEmpty(x.itemData.ItemName) &&
                            x.itemData.ItemName.ToLower().Contains(keyword.ToLower()))
                .Select(x => x.itemData)
                .ToList();

            if (matched.Count == 0)
            {
                Debug.LogWarning($"[ItemDatabase] No items found whose name contains '{keyword}'.");
            }
            else
            {
                Debug.Log($"[ItemDatabase] Found {matched.Count} items whose name contains '{keyword}':\n"
                          + string.Join(", ", matched.Select(i => i.ItemName)));
            }
            return matched;
        }

        // The method EnsureItemID has been removed so that ItemData.ItemID is not modified by this database.
        
        //==================================
        // Checks if an item is already in the database
        //==================================
        private bool IsAlreadyInDatabase(ItemData data)
        {
            return Items.Any(w => w.itemData == data);
        }
    }

    /// <summary>
    /// WrappedItem is used to display an <see cref="ItemData"/> inline in a [TableList].
    /// This allows us to see and edit each ItemData's fields directly in the Inspector.
    /// </summary>
    [System.Serializable]
    public class WrappedItem
    {
        [InlineEditor(InlineEditorObjectFieldModes.Foldout, Expanded = false)]
        public ItemData itemData;
    }

    //==================================
    // Classes for JSON Export
    //==================================

    /// <summary>
    /// A container class for serializing the entire database to JSON
    /// </summary>
    [System.Serializable]
    public class ItemDatabaseExport
    {
        public List<ItemExportData> items;
    }

    /// <summary>
    /// Defines which fields each item will include upon export
    /// </summary>
    [System.Serializable]
    public class ItemExportData
    {
        public string itemID;
        public string itemName;
        public string itemDescription;
        public string itemType;
        public int maxStackSize;
        // Add more fields as needed
    }
}