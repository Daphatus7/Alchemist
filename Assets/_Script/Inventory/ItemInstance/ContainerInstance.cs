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
        public PlayerContainer AssociatedContainer { get; internal set; }

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

        public override ItemSave OnSaveData()
        {
            var containerSave = new ContainerItemSave(this, AssociatedContainer.OnSaveData());
            return containerSave;
        }
    }
    
    public class ContainerItemSave : ItemSave
    {
        public InventorySave InventorySave { get; private set; }
        
        public ContainerItemSave(ContainerItemInstance itemInstance, InventorySave inventorySave) : base(itemInstance)
        {
            InventorySave = inventorySave;
        }
        
        public override ItemInstance InitializeItem(ItemInstance newInstance)
        {
            base.InitializeItem(newInstance);
            if (newInstance is ContainerItemInstance containerInstance)
            {
                containerInstance.AssociatedContainer = 
                    new PlayerContainer(null, InventorySave.width, InventorySave.height);
                containerInstance.AssociatedContainer.
                    OnLoadData(InventorySave.items);
            }
            return newInstance;
        }
    }
}