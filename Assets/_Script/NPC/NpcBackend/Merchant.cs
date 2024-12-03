// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 40

using _Script.Inventory.NpcInventoryBackend;

namespace _Script.NPC.NpcBackend
{
    public class Merchant : Npc
    {
        // Merchant specific properties
        public MerchantInventory merchantInventory;
        
        public void Start()
        {
            merchantInventory = GetComponent<MerchantInventory>();
        }
    }
}