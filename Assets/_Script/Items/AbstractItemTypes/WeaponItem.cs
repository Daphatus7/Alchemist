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

        public override void Use(PlayerCharacter playerCharacter)
        {
            
        }

        /// <summary>
        /// Don't make tangled logic - let one method handles only one thing
        /// </summary>
        /// <param name="playerCharacter"></param>
        public override void OnSelected(PlayerCharacter playerCharacter)
        {
            Debug.Log("Weapon Selected");
            //spawn the weapon
            //Let player handle the weapon
            playerCharacter.WeaponStrategy.ChangeWeapon(weaponPrefab, this);
            //Set Strategy
            playerCharacter.SetWeaponStrategy();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="playerCharacter"></param>
        public override void OnDeselected(PlayerCharacter playerCharacter)
        {
            //could this leads to deleting wrong item
            playerCharacter.WeaponStrategy.RemoveWeapon();
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