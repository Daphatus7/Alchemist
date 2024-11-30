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
        
        public GameObject cropPrefab;

        public override void Use(PlayerCharacter playerCharacter)
        {
            base.Use(playerCharacter);
        }
        
        public override void OnSelected(PlayerCharacter playerCharacter)
        {
            playerCharacter.GenericStrategy.ChangeItem(this);
            playerCharacter.SetGenericStrategy();
        }
        
        public override void OnDeselected(PlayerCharacter playerCharacter)
        {
            playerCharacter.GenericStrategy.RemoveItem();
            playerCharacter.UnsetStrategy();
        }
    }
}