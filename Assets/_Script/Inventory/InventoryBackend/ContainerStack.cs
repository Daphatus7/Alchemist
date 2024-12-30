// Author : Peiyu Wang @ Daphatus
// 12 12 2024 12 12

using _Script.Items;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public class ContainerItemStack : ItemStack
    {
        public PlayerContainer AssociatedContainer { get; private set; }

        // This constructor assumes you already have a PlayerContainer instance ready
        public ContainerItemStack(Vector2Int pivotPosition, ContainerItem itemData, int quantity, PlayerContainer container)
            : base(pivotPosition, itemData, quantity)
        {
            AssociatedContainer = container;
            if (AssociatedContainer == null)
            {
                // If no container provided, create a new one
                AssociatedContainer = new PlayerContainer(null, itemData.width, itemData.height);
            }
//            Debug.Log("ContainerItemStack created with Container ID: " + AssociatedContainer.UniqueID);
        }
    }
}