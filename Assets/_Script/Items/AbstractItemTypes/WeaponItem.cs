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
        
        public override void OnSelected(PlayerCharacter playerCharacter)
        {
            Debug.Log("Weapon Selected");
            //spawn the weapon
            //Let player handle the weapon
            playerCharacter.SetPlayerActionStrategy(ActionStrategyFactory.CreateStrategy(this));
        }
        
        public override void OnDeselected(PlayerCharacter playerCharacter)
        {
            //Destroy the handle - so the player cannot attack
            //Remove the visual representation of the weapon
            Debug.Log("Weapon Deselected");
            //remove existing weapon
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