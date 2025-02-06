// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 21

using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.AlchemyInventory
{
    public sealed class CauldronContainer : InventoryBackend.Inventory
    {
        public CauldronContainer(int height, int width) : base(height, width)
        {
        }

        public CauldronContainer(int height, int width, ItemStack[] itemStack) : base(height, width, itemStack)
        {
        }

        public override SlotType SlotType => SlotType.Cauldron;
        public override void LeftClickItem(int slotIndex)
        {
            
        }
    }
}