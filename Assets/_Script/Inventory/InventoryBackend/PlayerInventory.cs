using _Script.Inventory.InventoryHandles;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public class PlayerInventory : Inventory, IPlayerInventoryHandle
    {
        public bool Handle_AddItem(InventoryItem inventoryItem)
        {
            return AddItem(inventoryItem);
        }

        public bool Handle_RemoveItem(InventoryItem inventoryItem)
        {
            return RemoveItem(inventoryItem);
        }
    }
}