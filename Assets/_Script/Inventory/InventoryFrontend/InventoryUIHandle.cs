using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;
using _Script.Items;

namespace _Script.Inventory.InventoryFrontend
{
    public interface IContainerUIHandle
    {
        public void OnSlotClicked(InventorySlotDisplay slotDisplay);
        
        public InventoryItem RemoveAllItemsFromSlot(int slotIndex);
        public void AddItemToEmptySlot(InventoryItem item, int slotIndex);
    }
}