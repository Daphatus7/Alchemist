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
        public ItemAndQuantity[] reagents;
        /// <summary>
        /// the main output item
        /// </summary>
        public PotionBase mainOutputItem;
        public PotionCategory PotionCategory => mainOutputItem.PotionCategory;
        /// <summary>
        /// the possible secondary output items
        /// such as waste and byproducts
        /// </summary>
        public Byproduct[] secondaryOutputItems;
        
        public int outputQuantity = 1;
        public float craftingTime = 3;
    }
    
    /// <summary>
    /// Byproduct of the recipe
    /// </summary>
    [System.Serializable]
    public class Byproduct
    {
        public ItemData item;
        /// <summary>
        /// If the chance is greater than 1, it will generate more than 1 of the item
        /// </summary>
        public float chance;
    }
}