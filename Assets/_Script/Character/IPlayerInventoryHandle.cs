using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryHandles;

namespace _Script.Character
{
    public interface IPlayerHandler
    {
        public IPlayerInventoryHandle GetPlayerInventory();
        public IPlayerEquipmentHandle GetPlayerEquipment();
    }
}