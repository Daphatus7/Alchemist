// Author : Peiyu Wang @ Daphatus
// 21 02 2025 02 53

using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class ItemInstanceFactory
    {
        
        /// <summary>
        /// Split the item stack into two stacks. The original stack will have the quantity of the split stack subtracted from it.
        /// </summary>
        /// <param name="itemInstance"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static ItemInstance Split(ItemInstance itemInstance, int quantity)
        {
            return itemInstance.Split(quantity);
        }
        
        public static ItemInstance CreateNoPositionItemInstance(ItemData itemData, int quantity = 1)
        {
            //get item type
            var itemTypeString = itemData.ItemTypeString;
            switch (itemTypeString)
            {
                case "ContainerItem":
                    //1. create container item instance
                    var containerInstance = new ContainerItemInstance((ContainerItem)itemData, quantity);
                    if (containerInstance == null)
                    {
                        throw new NullReferenceException("ContainerItemInstance is null");
                    }
                    return containerInstance; 
                case "ConsumableItem":
                    return new ConsumableItemInstance((ConsumableItem)itemData, quantity);
                case "EquipmentItem":
                    return new EquipmentInstance((EquipmentItem)itemData, quantity);
                default:
                    throw new NotImplementedException("Item type not implemented");
            }
        }
        public static ItemInstance CreateItemInstance(List<Vector2Int> positions, ItemInstance oldItemInstance, int quantity = 1)
        {
            var newItemInstance = oldItemInstance.Split(quantity);
            newItemInstance.ItemPositions = positions;
            return newItemInstance;
        }
        
        public static ItemInstance CreateItemInstance(string itemID, int quantity = 1)
        {
            //get item type
            throw new NotImplementedException("item data base not implemented");
        }

        public static ItemInstance CreateItemInstance(string containerItemId, PlayerContainer container)
        {
            throw new NotImplementedException("item data base not implemented");
        }
    }
}