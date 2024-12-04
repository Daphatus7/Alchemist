using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.InventoryFrontendHandler
{
    public interface IContainerUIHandle
    {
        public void OnSlotClicked(InventorySlotDisplay slotDisplay);
        public InventoryItem RemoveAllItemsFromSlot(int slotIndex);
        public void AddItemToEmptySlot(InventoryItem item, int slotIndex);
        public InventoryItem AddItem(InventoryItem item);
        bool AcceptsItem(InventoryItem item);
    }
}