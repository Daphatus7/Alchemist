// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 50

using _Script.Character;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.SlotFrontend;

namespace _Script.Inventory.BagBackend
{
    public class Bag : PlayerContainer
    {
        public Bag(PlayerCharacter owner, int capacity) : base(owner, capacity)
        {
            
        }

        public override SlotType SlotType => SlotType.Bag;
        
    }
}