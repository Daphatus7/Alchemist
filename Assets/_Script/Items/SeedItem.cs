using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Seed Item", menuName = "Items/Seed Item")]
    public class SeedItem : MaterialItem
    {
        public override ItemType ItemType => ItemType.Seed;

        public override void Use(PlayerCharacter playerCharacter)
        {
            base.Use(playerCharacter);
        }
    }
}