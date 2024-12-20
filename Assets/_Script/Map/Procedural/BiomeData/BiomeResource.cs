// Author : Peiyu Wang @ Daphatus
// 20 12 2024 12 12

using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map.Procedural.BiomeData
{
    public class BiomeResource : ScriptableObject
    {
        [System.Serializable]
        public class BiomeResourceData
        {
            [HorizontalGroup("Row", Width = 60)]
            [HideLabel, PreviewField(50)]
            [ShowIf("@this.item != null")]
            [ShowInInspector] // Display as a property
            private Sprite ItemIcon => item ? item.itemIcon : null;

            [HorizontalGroup("Row")]
            [ReadOnly, LabelText("Item Name")]
            [ShowInInspector]
            private string ItemName => item ? item.itemName : "No Item";

            [HorizontalGroup("Item")]
            [LabelText("Item")]
            public ItemData item;
            
            [VerticalGroup("Drop Info")]
            [LabelText("Unique")]
            public bool isUnique;
        }

        [TableList(AlwaysExpanded = true)]
        public BiomeResourceData[] resources; // Array of possible drops for this table
        
        public IEnumerable<BiomeResourceData> GetResources()
        {
            return resources;
        }
    }
    
}