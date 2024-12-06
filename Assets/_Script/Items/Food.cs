// Author : Peiyu Wang @ Daphatus
// 05 12 2024 12 57

using System;
using System.Collections.Generic;
using _Script.Character;
using UnityEngine;
using Sirenix.OdinInspector;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Food", menuName = "Items/Consumable/Food")]
    public class Food : ConsumableItem
    {
        [ShowInInspector]
        public Dictionary<FoodType, int> foodValue = new Dictionary<FoodType, int>();
        
        public override bool Use(PlayerCharacter playerCharacter)
        {
            foreach (var pair in foodValue)
            {
                playerCharacter.EatFood(pair.Key, pair.Value);
            }
            return true;
        }
    }
    
    public enum FoodType
    {
        Health,
        Mana,
        Stamina,
        Sanity,
        Hunger
    }
}