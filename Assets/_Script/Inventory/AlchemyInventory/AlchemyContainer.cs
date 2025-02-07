// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 21

using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.AlchemyInventory
{
    public class AlchemyContainer : InventoryBackend.Inventory
    {
        public AlchemyContainer(int height = 3, int width = 3) : base(height, width)
        {
        }

        public AlchemyContainer(int height, int width, ItemStack[] itemStack) : base(height, width, itemStack)
        {
        }

        public override SlotType SlotType => SlotType.Cauldron;
    }
}