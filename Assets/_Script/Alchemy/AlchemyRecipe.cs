// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 19

using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Alchemy
{
    [CreateAssetMenu(fileName = "AlchemyRecipe", menuName = "Items/Alchemy/AlchemyRecipe")]
    public class AlchemyRecipe : ScriptableObject
    {
        public string recipeID;
        public string recipeName;

        public ItemData[] requiredIngredients;
        public ItemData[] outputItems;
        
        public int outputQuantity = 1;

        public float craftingTime = 3;
    }
}