using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryHandles;

namespace _Script.Character
{
    public interface IPlayerInventoryHandler
    {
        public IPlayerInventoryHandle GetPlayerInventory();
        public IPlayerEquipmentHandle GetPlayerEquipment();
    }
}