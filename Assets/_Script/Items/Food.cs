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
        
        [SerializeField]
        private List<FoodEffect> foodEffects = new List<FoodEffect>();

        public override bool Use(PlayerCharacter playerCharacter)
        {
            foreach (var effect in foodEffects)
            {
                playerCharacter.OnFoodConsumed(effect);
            }
            return true;
        }
    }

    [Serializable]
    public class FoodEffect
    {
        /// <summary>
        /// Food type
        /// </summary>
        public FoodType FoodType;
        /// <summary>
        /// Food value
        /// </summary>
        public int Value;
    }

    public enum FoodType
    {
        Health,
        Mana,
        Stamina,
        Sanity,
        Food
    }
}