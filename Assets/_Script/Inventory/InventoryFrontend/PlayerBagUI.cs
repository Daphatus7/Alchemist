// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 44

using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Inventory.SlotFrontend;
using UnityEngine;

namespace _Script.Inventory.InventoryFrontend
{
    public class PlayerBagUI : MonoBehaviour, IPlayerInventoryHandler
    {
        private List<PlayerInventory.PlayerInventory> _playerInventories;
        

        public void OnSlotClicked(InventorySlotDisplay slotDisplay)
        {
            throw new System.NotImplementedException();
        }

        public InventoryItem RemoveAllItemsFromSlot(int slotIndex)
        {
            throw new System.NotImplementedException();
        }

        public void AddItemToEmptySlot(InventoryItem item, int slotIndex)
        {
            throw new System.NotImplementedException();
        }

        public InventoryItem AddItem(InventoryItem item)
        {
            throw new System.NotImplementedException();
        }

        public bool AcceptsItem(InventoryItem item)
        {
            throw new System.NotImplementedException();
        }

        public void AddGold(int amount)
        {
            throw new System.NotImplementedException();
        }

        public bool RemoveGold(int amount)
        {
            throw new System.NotImplementedException();
        }
    }
}