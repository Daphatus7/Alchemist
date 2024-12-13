using _Script.Character;
using UnityEngine;
using Sirenix.OdinInspector; // Import Odin

namespace _Script.Items.AbstractItemTypes
{
    namespace _Script.Items
    {
        [System.Serializable]
        public abstract class ItemData : ScriptableObject
        {
            [Title("Basic Info")]
            [SerializeField, Tooltip("Name of the item")]
            public string itemName;

            [SerializeField, Tooltip("Unique ID of the item")]
            public string itemID;

            [SerializeField, TextArea, Tooltip("Detailed description of the item")]
            public string itemDescription;

            [Title("Visuals")]
            [SerializeField, Tooltip("Icon representing the item"), PreviewField(75)]
            public Sprite itemIcon;

            [Title("Stacking & Rarity")]
            [SerializeField, Tooltip("Maximum stack size for this item")]
            public int maxStackSize = 1;
            

            [SerializeField, Tooltip("Rarity of the item")]
            public Rarity rarity;

            [SerializeField] private int _value = 1; public int Value
            {
                get => _value;
                set => _value = value;
            }

            [Title("Read-Only Debug Info"), ReadOnly, ShowInInspector]
            public Sprite ItemSprite => itemIcon;

            [ReadOnly, ShowInInspector]
            public string ItemName
            {
                get => itemName;
                set => itemName = value;
            }

            [ReadOnly, ShowInInspector]
            public string ItemID
            {
                get => itemID;
                set => itemID = value;
            }

            [ReadOnly, ShowInInspector]
            public string ItemDescription
            {
                get => itemDescription;
                set => itemDescription = value;
            }

            [ReadOnly, ShowInInspector]
            public int MaxStackSize => maxStackSize;

            public abstract ItemType ItemType { get; }
            public abstract string ItemTypeString { get; }

            /// <summary>
            /// Use the item. Applies effects to the player (e.g., equip, consume).
            /// </summary>
            /// <param name="playerCharacter">The player character to apply effects to.</param>
            /// <returns>True if the item was used successfully; false otherwise.</returns>
            public abstract bool Use(PlayerCharacter playerCharacter);
        }
        public enum ItemType
        {
            Equipment,
            Consumable,
            Material,
            Seed,
            Fruit,
            Torch,
            Container
        }
        
        public enum Rarity
        {
            Common,
            Uncommon,
            Rare,
            Epic,
            Legendary
        }
    }
}