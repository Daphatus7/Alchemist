// Author : Peiyu Wang @ Daphatus
// 24 02 2025 02 52

using System;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.PlayerInventory;

namespace _Script.Character
{
    [Serializable]
    public class PlayerSave
    {
        public PlayerInventorySave PlayerInventory;
        public PlayerStatsSave stats;
        public int gold;
        public PlayerSave(PlayerInventorySave playerInventory, 
            PlayerStatsSave stats, 
            int gold)
        {
            PlayerInventory = playerInventory;
            this.stats = stats;
            this.gold = gold;
        }
    }


}