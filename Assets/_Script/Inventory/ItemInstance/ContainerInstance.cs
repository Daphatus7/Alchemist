// Author : Peiyu Wang @ Daphatus
// 12 12 2024 12 12

using _Script.Inventory.InventoryBackend;
using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class ContainerItemInstance : ItemInstance
    {
        public PlayerContainer AssociatedContainer { get; private set; }

        // This constructor assumes you already have a PlayerContainer instance ready
        public ContainerItemInstance(ContainerItem itemData, int quantity, PlayerContainer container = null)
            : base(itemData, quantity)
        {
            AssociatedContainer = container;
            if (AssociatedContainer == null)
            {
                // If no container provided, create a new one
                AssociatedContainer = new PlayerContainer(null, itemData.width, itemData.height);
            }
            Debug.Log("ContainerItemStack created with Container ID: " + AssociatedContainer.UniqueID);
        }
    }
}