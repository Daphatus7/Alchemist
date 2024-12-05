// Author : Peiyu Wang @ Daphatus
// 05 12 2024 12 46

using System.Collections;
using _Script.Character;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Town Scroll", menuName = "Items/Consumable/Town Scroll")]
    public class Scroll : ConsumableItem
    {
        public float castTime = 10f;
        public ScrollType scrollType;
        
        public override bool Use(PlayerCharacter playerCharacter)
        {
            if (IsInCombat(playerCharacter))
            {
                return false;
            }
            else
            {
                return playerCharacter.UseTownScroll(scrollType, castTime);
            }
        }

        private bool IsInCombat(PlayerCharacter playerCharacter)
        {
            return false;
        }
    }
    
    public enum ScrollType
    {
        Town,
        Dungeon
    }
}