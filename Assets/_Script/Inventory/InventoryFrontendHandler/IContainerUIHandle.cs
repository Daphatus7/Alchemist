using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontendHandler
{
    public interface IContainerUIHandle
    {
        public void OnSlotClicked(InventorySlotInteraction slotInteraction);
        public ItemStack RemoveAllItemsFromSlot(int slotIndex);
        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex);
        public ItemStack AddItem(ItemStack itemStack);
        bool AcceptsItem(ItemStack itemStack);
        
        bool CanFitItem(List<Vector2Int> projectedPositions);
        
        Vector2Int GetSlotPosition(int slotIndex);
        
        int GetSlotIndex(Vector2Int position);
        int GetItemsCount(int shiftedPivotIndex //the location of the user selected
            , List<Vector2Int> peakItemStack //the offset locations of the item
            , out int onlyItemIndex //current not implemented, supposed to handle when there is an item so the player can swap
            );
    }
}