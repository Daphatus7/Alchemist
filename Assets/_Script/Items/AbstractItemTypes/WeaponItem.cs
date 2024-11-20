using _Script.Character;
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

        public override void Use(PlayerCharacter playerCharacter)
        {
            GameObject weapon = Instantiate(weaponPrefab);
            Debug.Log("Equipping weapon" + weapon.name);
            playerCharacter.EquipWeapon(weapon);
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