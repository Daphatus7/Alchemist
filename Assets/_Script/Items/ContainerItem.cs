using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Container Item", menuName = "Items/Container Item")]
    public class ContainerItem : ItemData
    {
        public int width = 3;
        public int height = 3;
        public override ItemType ItemType => ItemType.Container;
        public override string ItemTypeString => "Container";

        public override bool Use(PlayerCharacter playerCharacter)
        {
            return true;
        }
    }
}