using _Script.Character;
using _Script.Character.ActionStrategy;
using _Script.Items.AbstractItemTypes;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Helmet Item", menuName = "Items/Equipments/Armours/Helmets")]
    public class HelmetItem : ArmourItem
    {
        public override string ItemTypeString => "Helmet";
        public override void Use(PlayerCharacter playerCharacter)
        {
            throw new System.NotImplementedException();
        }
    }
}