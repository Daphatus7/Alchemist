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
            [SerializeField] private string itemName;
            [SerializeField] private int itemID;
            [SerializeField, TextArea] private string itemDescription;
            [SerializeField] private Sprite itemIcon;
            [SerializeField] private int maxStackSize = 1;

            // Public read-only properties
            public string ItemName => itemName;
            public int ItemID => itemID;
            public string ItemDescription => itemDescription;
            public Sprite ItemIcon => itemIcon;
            public int MaxStackSize => maxStackSize;

            public abstract ItemType ItemType { get; }

            
            /**
             * Use the item, When using an item, this applies effect to the player either by equipping, consuming etc.
             */
            public abstract void Use(PlayerCharacter playerInventoryCharacter);
        }

        public enum ItemType
        {
            Equipment,
            Consumable,
            Material,
        }
    }
}