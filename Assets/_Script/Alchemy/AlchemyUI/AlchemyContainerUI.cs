// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 46

using _Script.Inventory.AlchemyInventory;
using _Script.Inventory.InventoryFrontendBase;

namespace _Script.Alchemy.AlchemyUI
{
    public class AlchemyContainerUI : InventoryUIBase<AlchemyContainer>
    {
        //a special container just for the cauldrons
        //player cannot drag thing into the Cauldron
        //A Cauldron item will be added via the backend
        
        public void LoadContainer(AlchemyContainer alchemyContainer)
        {
            AssignInventory(alchemyContainer);
            InitializeInventoryUI();
            ShowUI();
        }
        
    }
}