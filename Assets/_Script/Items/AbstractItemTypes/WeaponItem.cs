using _Script.Character;
using _Script.Character.ActionStrategy;
using UnityEngine;

namespace _Script.Items.AbstractItemTypes
{
    public abstract class WeaponItem : EquipmentItem
    {
        public WeaponType weaponType = WeaponType.None;
        public override EquipmentType EquipmentType => EquipmentType.Weapon;
        public float damage = 1;
        public float attackSpeed = 1f;
        public float range = 1f;
        public GameObject weaponPrefab;
        
        public override string ItemTypeString => "Weapon";

        public override void Use(PlayerCharacter playerCharacter)
        {
            
        }
    }

    public enum WeaponType
    {
        Sword,
        Axe,
        Staff,
        Wand,
        Pickaxe,
        Hammer,
        None,
    }
}