using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.InventoryFrontendHandler
{
    public interface IContainerUIHandle
    {
        public void OnSlotClicked(InventorySlotDisplay slotDisplay);
        public ItemStack RemoveAllItemsFromSlot(int slotIndex);
        public void AddItemToEmptySlot(ItemStack itemStack, int slotIndex);
        public ItemStack AddItem(ItemStack itemStack);
        bool AcceptsItem(ItemStack itemStack);
        
        bool CanFitItem(int targetSlotIndex, ItemStack comparingItemStack);
    }
}