using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontendHandler
{
    public interface IContainerUIHandle
    {
        public void OnSlotClicked(InventorySlotInteraction slotInteraction);
        public ItemInstance.ItemInstance RemoveAllItemsFromSlot(int slotIndex);
        public void AddItemToEmptySlot(ItemInstance.ItemInstance itemInstance, List<Vector2Int> projectedPositions);
        public ItemInstance.ItemInstance AddItem(ItemInstance.ItemInstance itemInstance);
        bool AcceptsItem(ItemInstance.ItemInstance itemInstance);
        
        bool CanFitItem(List<Vector2Int> projectedPositions);
        
        Vector2Int GetSlotPosition(int slotIndex);
        
        int GetSlotIndex(Vector2Int position);
        int GetItemsCount(int shiftedPivotIndex //the location of the user selected
            , List<Vector2Int> peakItemStack //the offset locations of the item
            , out int onlyItemIndex //current not implemented, supposed to handle when there is an item so the player can swap
            );
    }
}