using _Script.Character;
using _Script.Inventory.InventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable")]
    public class ConsumableItem : ItemData
    {
        public int amount;
        public ConsumableType consumableType;
        public override ItemType ItemType => ItemType.Consumable;
        public override void Use(PlayerCharacter playerCharacter)
        {
            
        }
        
    }
    
    public enum ConsumableType
    {
        HealthPotion,
        ManaPotion,
        StaminaPotion
    }
}