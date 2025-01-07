using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    public enum EquipmentType
    {
        Armour,
        Weapon,
        Accessory,
        Torch
    }

    public abstract class EquipmentItem : ItemData
    {
        public abstract EquipmentType EquipmentType { get; }
        public override ItemType ItemType => ItemType.Equipment;
        
        public virtual void RemoveEffect(PlayerCharacter playerCharacter)
        {
            // Remove the effect of the item
        }
    }
}