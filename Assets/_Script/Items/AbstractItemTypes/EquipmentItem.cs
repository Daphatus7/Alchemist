using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    public enum EquipmentType
    {
        Armour,
        Weapon,
        Accessory
    }

    public abstract class EquipmentItem : ItemData
    {
        public abstract EquipmentType EquipmentType { get; }
        public override ItemType ItemType => ItemType.Equipment;
        public override void Use(PlayerCharacter playerCharacter)
        {
            // Equip the item
            InventoryItem returnItem = playerCharacter.GetPlayerEquipment().Handle_Equip_ApplyEffect(new InventoryItem(this));
            if (returnItem != null)
            {
                playerCharacter.GetPlayerInventory().Handle_AddItem(returnItem);
            }
        }
    }
}