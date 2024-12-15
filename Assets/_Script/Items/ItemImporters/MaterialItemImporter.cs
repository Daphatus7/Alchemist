// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 11

using System.IO;
using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEditor;
using UnityEngine;

namespace _Script.Items.ItemImporters
{
    public static class MaterialItemImporter
    {
        private static Dictionary<string, Sprite> _spriteDictionary;

        [MenuItem("Tools/Import Materials From JSON")]
        public static void ImportMaterialsFromJson()
        {
            // Prompt user to select the JSON file
            string path = EditorUtility.OpenFilePanel("Select Material JSON", "", "json");
            if (string.IsNullOrEmpty(path)) return;

            string jsonContent = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<MaterialItemJsonWrapper>(jsonContent);
            if (wrapper == null || wrapper.materials == null)
            {
                Debug.LogError("No valid material data found in JSON.");
                return;
            }

            // Ensure folder structure
            string prefabsFolder = "Assets/_Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabsFolder))
            {
                AssetDatabase.CreateFolder("Assets", "_Prefabs");
            }

            string itemsFolder = "Assets/_Prefabs/Items";
            if (!AssetDatabase.IsValidFolder(itemsFolder))
            {
                AssetDatabase.CreateFolder(prefabsFolder, "Items");
            }

            string folderPath = "Assets/_Prefabs/Items/Materials";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(itemsFolder, "Materials");
            }

            // One-time sprite search from multiple directories
            BuildSpriteDictionary();

            foreach (var entry in wrapper.materials)
            {
                ImportOrUpdateMaterialItem(entry, folderPath);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Materials imported successfully.");
        }

        private static void ImportOrUpdateMaterialItem(MaterialItemJsonEntry entry, string folderPath)
        {
            // Search for an existing item by ItemID in the prefabs folder
            string[] guids = AssetDatabase.FindAssets(entry.ItemID, new[] {folderPath});

            MaterialItem materialItem;
            if (guids.Length > 0)
            {
                // Load existing MaterialItem
                string assetPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                materialItem = AssetDatabase.LoadAssetAtPath<MaterialItem>(assetPath);
            }
            else
            {
                // Create a new MaterialItem if not found
                materialItem = ScriptableObject.CreateInstance<MaterialItem>();
                AssetDatabase.CreateAsset(materialItem, $"{folderPath}/{entry.ItemName}.asset");
            }

            // Update fields
            materialItem.ItemName = entry.ItemName;
            materialItem.ItemDescription = entry.Description;
            materialItem.ItemID = entry.ItemID;
            materialItem.maxStackSize = entry.MaxStack;
            materialItem.Value = entry.Value;
            materialItem.rarity = entry.GetRarity();

            // Assign sprite if available
            if (_spriteDictionary != null && _spriteDictionary.TryGetValue(entry.ItemName, out var foundSprite))
            {
                materialItem.itemIcon = foundSprite;
            }
            else
            {
                // Optional: Log if no sprite found
                Debug.LogWarning($"No sprite found for item '{entry.ItemName}'");
            }

            EditorUtility.SetDirty(materialItem);
        }

        private static void BuildSpriteDictionary()
        {
            _spriteDictionary = new Dictionary<string, Sprite>();

            // Array of folders that may contain sprites
            string[] foldersToSearch = new[]
            {
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Equipment",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Food",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Material",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Misc",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Monster Part",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Ore & Gem",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Potion",
                "Assets/Cainos/Pixel Art Icon Pack - RPG/Texture/Weapon & Tool"
            };

            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", foldersToSearch);
            foreach (var guid in spriteGuids)
            {
                string spritePath = AssetDatabase.GUIDToAssetPath(guid);
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
                if (sprite != null)
                {
                    string spriteKey = Path.GetFileNameWithoutExtension(sprite.name);
                    _spriteDictionary[spriteKey] = sprite;
                }
            }
        }
    }

    [System.Serializable]
    public class MaterialItemJsonEntry
    {
        public string ItemName;
        public string Description;
        public string ItemID;
        public int MaxStack;
        public int Value;
        public string Rarity;

        public Rarity GetRarity()
        {
            switch (Rarity)
            {
                case "Common":
                    return AbstractItemTypes._Script.Items.Rarity.Common;
                case "Uncommon":
                    return AbstractItemTypes._Script.Items.Rarity.Uncommon;
                case "Rare":
                    return AbstractItemTypes._Script.Items.Rarity.Rare;
                case "Epic":
                    return AbstractItemTypes._Script.Items.Rarity.Epic;
                case "Legendary":
                    return AbstractItemTypes._Script.Items.Rarity.Legendary;
                default:
                    return AbstractItemTypes._Script.Items.Rarity.Common;
            }
        }
    }

    [System.Serializable]
    public class MaterialItemJsonWrapper
    {
        public MaterialItemJsonEntry[] materials;
    }
}