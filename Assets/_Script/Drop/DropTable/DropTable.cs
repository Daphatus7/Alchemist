// Author : Peiyu Wang @ Daphatus
// 13 12 2024 12 22

using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Drop.DropTable
{
    [CreateAssetMenu(fileName = "DropTable", menuName = "GameData/DropTable")]
    public class DropTable : ScriptableObject, IDropProvider 
    {
        [System.Serializable]
        public class DropItem
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
            public float dropChance = 0.1f;

            [VerticalGroup("Drop Info")]
            public int minAmount = 1;

            [VerticalGroup("Drop Info")]
            public int maxAmount = 1;

            [VerticalGroup("Drop Info")]
            [LabelText("Unique")]
            public bool isUnique;
        }

        [TableList(AlwaysExpanded = true)]
        public DropItem[] drops; // Array of possible drops for this table
        
        public IEnumerable<DropItem> GetDrops()
        {
            return drops;
        }
    }
}