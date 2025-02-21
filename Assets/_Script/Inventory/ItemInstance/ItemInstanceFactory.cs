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
    }
}