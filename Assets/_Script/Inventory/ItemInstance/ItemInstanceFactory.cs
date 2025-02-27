// Author : Peiyu Wang @ Daphatus
// 21 02 2025 02 53

using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Managers;
using _Script.Managers.Database;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class ItemInstanceFactory
    {
        /// <summary>
        /// Split the item stack into two stacks. The original stack will have the quantity of the split stack subtracted from it.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static ItemInstance Split(ItemInstance source, int quantity)
        {
            //if quantity is greater than the source quantity, error, because this should not happen
            if (quantity > source.Quantity)
            {
                throw new Exception("quantity is greater than the source quantity");
            }
            //if quantity is less but not equal to the source quantity, split the stack
            else if (quantity < source.Quantity)
            {
                return source.Split(quantity);
            }
            else //where A == B
            {
                return source;
            }
        }
        
        /// <summary>
        /// Create an item instance from the item data
        /// Instance create from this class only create default items instead of manipulating existing item instance
        /// such as copying and spliting
        /// </summary>
        /// <param name="itemData"></param>
        /// <param name="rotated"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        public static ItemInstance CreateItemInstance(ItemData itemData, bool rotated, int quantity)
        {
            var itemTypeString = itemData.ItemTypeString;
            Debug.Log($"Creating item instance of type {itemTypeString}");
            return itemTypeString switch
            {
                "Weapon" => new WeaponItemInstance(itemData, rotated, quantity),
                "Torch" => new TorchItemInstance(itemData, rotated, quantity),
                "Seed" => new SeedItemInstance(itemData, rotated, quantity),
                "Container" => new ContainerItemInstance(itemData, rotated, quantity),
                _ => new ItemInstance(itemData, rotated, quantity)
            };
        }
        
        /// <summary>
        /// Used to recreate an item instance from the save data
        /// </summary>
        /// <param name="itemSave"></param>
        /// <returns></returns>
        public static ItemInstance RecreateItemInstanceSave(ItemSave itemSave)
        {
            var itemData = DatabaseManager.Instance.GetItemData(itemSave.itemID);
            var itemInstance = CreateItemInstance(itemData, itemSave.rotated, itemSave.quantity);
            itemSave.InitializeItem(itemInstance);
            return itemInstance;
        }
    }
}