using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Container Item", menuName = "Items/Container Item")]
    public class ContainerItem : ItemData
    {
        [SerializeField] private int _capacity = 6; 
        public int Capacity => _capacity;

        public override ItemType ItemType => ItemType.Container;
        public override string ItemTypeString => "Container";

        public override bool Use(PlayerCharacter playerCharacter)
        {
            return true;
        }
    }
}