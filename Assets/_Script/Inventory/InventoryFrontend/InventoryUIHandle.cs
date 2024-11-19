using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.InventoryFrontend
{
    public interface IContainerUIHandle
    {
        public void OnSlotClicked(InventorySlotDisplay slotDisplay);
    }
}