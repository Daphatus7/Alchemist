using _Script.Character;
using _Script.Items._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Consumable", menuName = "Items/Consumable")]
    public class ConsumableItem : ItemData
    {
        public int healthRestoreAmount;

        public override void Use(PlayerCharacter playerCharacter)
        {
        }
    }
}