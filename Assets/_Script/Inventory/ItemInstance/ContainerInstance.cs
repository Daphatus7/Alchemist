// Author : Peiyu Wang @ Daphatus
// 12 12 2024 12 12

using _Script.Inventory.InventoryBackend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class ContainerItemInstance : ItemInstance
    {
        public PlayerContainer AssociatedContainer { get; private set; }

        // This constructor assumes you already have a PlayerContainer instance ready
        public ContainerItemInstance(ItemData itemData, bool rotated, int quantity, PlayerContainer container = null)
            : base(itemData, rotated, quantity)
        {
            AssociatedContainer = container;
            if (AssociatedContainer == null && ItemData is ContainerItem containerData)
            {
                // If no container provided, create a new one
                AssociatedContainer = new PlayerContainer(null, containerData.width, containerData.height);
            }
            //Debug.Log("ContainerItemStack created with Container ID: " + AssociatedContainer.UniqueID);
        }
    }
}