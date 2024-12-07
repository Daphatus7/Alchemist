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
        private List<FoodValue> foodValues = new List<FoodValue>();

        public override bool Use(PlayerCharacter playerCharacter)
        {
            foreach (var foodValue in foodValues)
            {
                playerCharacter.EatFood(foodValue.FoodType, foodValue.Value);
            }
            return true;
        }

        // Helper to get dictionary-like behavior
        public Dictionary<FoodType, int> GetFoodValuesAsDictionary()
        {
            var dictionary = new Dictionary<FoodType, int>();
            foreach (var foodValue in foodValues)
            {
                dictionary[foodValue.FoodType] = foodValue.Value;
            }
            return dictionary;
        }
    }

    [Serializable]
    public class FoodValue
    {
        public FoodType FoodType;
        public int Value;
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