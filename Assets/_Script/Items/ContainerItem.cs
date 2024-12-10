// Author : Peiyu Wang @ Daphatus
// 09 12 2024 12 26

using _Script.Character;
using _Script.Inventory.BagBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Container Item", menuName = "Items/Container Item")]
    public class ContainerItem : ItemData
    {
        [SerializeField] private int _capacity = 6; public int Capacity => _capacity;

        private PlayerContainer _container; public PlayerContainer Container => _container;

        public ContainerItem()
        {
            _container = new PlayerContainer(null, _capacity);
        }
        
        public override ItemType ItemType => ItemType.Container;
        public override string ItemTypeString => "Container";
        public override bool Use(PlayerCharacter playerCharacter)
        {
            //load inventory
            Debug.Log("Loading container inventory...");
            return false;
        }
    }
}