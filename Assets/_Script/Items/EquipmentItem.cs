using _Script.Character;
using _Script.Items._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    public enum EquipmentType
    {
        Helmet,
        Chest,
        Legs,
        Weapon,
        Shield,
        Accessory
    }

    [CreateAssetMenu(fileName = "New Equipment", menuName = "Items/Equipment")]
    public class EquipmentItem : ItemData
    {
        public EquipmentType equipmentType;
        public int attackBonus;
        public int defenseBonus;

        public override void Use(PlayerCharacter playerCharacter)
        {
        }
    }
}