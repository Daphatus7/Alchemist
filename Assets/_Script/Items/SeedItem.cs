using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Seed Item", menuName = "Items/Material/Seed")]
    public class SeedItem : MaterialItem
    {
        public override ItemType ItemType => ItemType.Seed;
        
        public override string ItemTypeString => "Seed";

        public override void Use(PlayerCharacter playerCharacter)
        {
            base.Use(playerCharacter);
        }
        
        public override void OnSelected(PlayerCharacter playerCharacter)
        {
            Debug.Log("Selected Seed Item " + ItemName);
        }
        
        public override void OnDeselected(PlayerCharacter playerCharacter)
        {
            Debug.Log("Deselected Seed Item " + ItemName);
        }
    }
}