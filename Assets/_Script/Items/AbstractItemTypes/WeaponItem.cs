using _Script.Character;
using _Script.Character.ActionStrategy;
using UnityEngine;

namespace _Script.Items.AbstractItemTypes
{
    public abstract class WeaponItem : EquipmentItem
    {
        public WeaponType weaponType = WeaponType.None;
        public override EquipmentType EquipmentType => EquipmentType.Weapon;
        public int durability = 20;
        public float damageMin = 1;
        public float damageMax = 2;
        public float attackSpeed = 1f;
        public float range = 1f;
        public GameObject weaponPrefab;
        
        public override string ItemTypeString => "Weapon";

        public override bool Use(PlayerCharacter playerCharacter)
        {
            return true;
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
        Torch,
        None,
    }
}