using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Items.Helper;
using UnityEngine;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine.Serialization; // Import Odin

namespace _Script.Items.AbstractItemTypes
{
    namespace _Script.Items
    {
        [Serializable]
        public abstract class ItemData : ScriptableObject
        {
            [Title("Basic Info")] [SerializeField, Tooltip("Name of the item")]
            public string itemName;

            [SerializeField, Tooltip("Unique ID of the item")]
            public string itemID;

            [SerializeField, TextArea, Tooltip("Detailed description of the item")]
            public string itemDescription;

            [Title("Visuals")] [SerializeField, Tooltip("Icon representing the item"), PreviewField(75)]
            public Sprite itemIcon;

            [Title("Stacking & Rarity")] [SerializeField, Tooltip("Maximum stack size for this item")]
            public int maxStackSize = 1;
            
            public int MaxStackSize
            {
                get => maxStackSize;
                set => maxStackSize = value;
            }

            public ItemShapeType itemShapeType = ItemShapeType.Square11;
            private ItemShape _itemShape;

            public ItemShape ItemShape
            {
                get => _itemShape ??= new ItemShape(itemShapeType);
                set => _itemShape = value;
            }

            [SerializeField, Tooltip("Rarity of the item")]
            public Rarity rarity;


            [SerializeField] private int _value = 1;

            public int Value
            {
                get => _value;
                set => _value = value;
            }

            public int GetPivotIndex(bool isRotated)
            {
                return ItemShape.GetShapePivotIndex(itemShapeType, isRotated);
            }
            
            /// <summary>
            /// Offset for rendering display of the item.
            /// Hardcoded solution
            /// </summary>
            /// <param name="isRotated"></param>
            /// <returns></returns>
            public Vector3 GetRenderingOffset(bool isRotated)
            {
                return ItemShape.GetShapeRenderingOffset(itemShapeType, isRotated);
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

            [ReadOnly, ShowInInspector] public abstract ItemType ItemType { get; }
            public abstract string ItemTypeString { get; }

            /// <summary>
            /// Use the item. Applies effects to the player (e.g., equip, consume).
            /// </summary>
            /// <param name="playerCharacter">The player character to apply effects to.</param>
            /// <returns>True if the item was used successfully; false otherwise.</returns>
            public abstract bool Use(PlayerCharacter playerCharacter);

            public bool Equals(ItemData other)
            {
                return other != null && itemName == other.itemName;
            }
        }
        
        [Serializable]
        public class ItemAndQuantity
        {
            [SerializeField] private ItemData data; public ItemData Data => data;
            [SerializeField] private int quantity; public int Quantity => quantity;
            
            public ItemAndQuantity(ItemData data, int quantity)
            {
                this.data = data;
                this.quantity = quantity;
            }
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