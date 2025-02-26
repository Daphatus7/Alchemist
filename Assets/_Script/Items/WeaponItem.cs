using _Script.Character;
using _Script.Items.AbstractItemTypes;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Weapon", menuName = "Items/Equipments/Weapons/Tool")]
    public class WeaponItem : EquipmentItem
    {
        public WeaponType weaponType = WeaponType.None;
        public override EquipmentType EquipmentType => EquipmentType.Weapon;
        public AttackForm attackForm = AttackForm.Slash;
        public int durability = 20;
        public float damageMin = 1;
        public float damageMax = 2;
        public float attackCooldown = 1f;
        public float attackTime = 0.5f;
        //Range of the weapon
        public float attackDistance = 1f;
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
        Dagger,
        Wand,
        Pickaxe,
        Hammer,
        Torch,
        None,
    }
    public enum AttackForm
    {
        Hammer,
        Stab,
        Slash,
    }
}