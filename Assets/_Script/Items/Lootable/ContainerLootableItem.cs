// Author : Peiyu Wang @ Daphatus
// 12 12 2024 12 38

using System.Collections.Generic;
using _Script.Character;
using _Script.Inventory.InventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items.Lootable
{
    // A helper class to store which items and how many should be added
    [System.Serializable]
    public class DebugContainerItemEntry
    {
        public ItemData itemData;
        public int quantity = 1;
    }

    // A subclass of ItemLootable that specifically handles container items.
    // It creates and holds the PlayerContainer at spawn and passes it along on pickup.
    public class ContainerLootableItem : ItemLootable
    {
        [Header("Debug Items to Add")]
        [Tooltip("Items listed here will be added to the container for debugging/testing purposes.")]
        [SerializeField]
        private List<DebugContainerItemEntry> debugItems = new List<DebugContainerItemEntry>();

        private PlayerContainer _runtimeContainer;

        protected void Start()
        {
            if (itemData is ContainerItem containerItem)
            {
                _runtimeContainer = new PlayerContainer(null, containerItem.width, containerItem.height);

                // Add debug items to the runtime container
                foreach (var entry in debugItems)
                {
                    if (entry.itemData != null && entry.quantity > 0)
                    {
                        var remainder = _runtimeContainer.AddItem(new ItemStack(entry.itemData, entry.quantity));
                        if (remainder != null && !remainder.IsEmpty)
                        {
                            Debug.LogWarning($"Not all items could be added to the container. Remainder: {remainder.Quantity}x {remainder.ItemData.ItemName}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("ContainerLootableItem used with non-container itemData.");
            }
        }

        protected override void PickupItem(PlayerCharacter player)
        {
            if (player)
            {
                if (itemData is ContainerItem cItem && _runtimeContainer != null)
                {
                    // Create a ContainerItemStack with the pre-created container
                    var cStack = new ContainerItemStack(cItem, 1, _runtimeContainer);
                    // Attempt to add the container stack to the player's inventory
                    if (player.PlayerInventory.AddItem(cStack) == null)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        Debug.Log("Not enough space in inventory for container stack!");
                    }
                }
                else
                {
                    Debug.LogWarning("ContainerLootableItem used with non-container or missing container.");
                    base.PickupItem(player); // fallback to normal item logic
                }
            }
        }
    }
}