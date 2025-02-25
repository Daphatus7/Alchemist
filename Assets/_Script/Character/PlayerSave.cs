// Author : Peiyu Wang @ Daphatus
// 24 02 2025 02 52

using System;
using _Script.Character.PlayerRank;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.PlayerInventory;

namespace _Script.Character
{
    [Serializable]
    public class PlayerSave
    {
        public PlayerInventorySave playerInventory;
        public PlayerStatsSave stats;
        public int gold;
        public PlayerRankSave rankSave;
        public PlayerSave(PlayerInventorySave playerInventory, 
            PlayerStatsSave stats, 
            int gold, 
            PlayerRankSave rankSave)
        {
            this.playerInventory = playerInventory;
            this.stats = stats;
            this.gold = gold;
            this.rankSave = rankSave;
        }
    }


}