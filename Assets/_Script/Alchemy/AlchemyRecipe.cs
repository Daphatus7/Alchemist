// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 19

using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Alchemy
{
    [CreateAssetMenu(fileName = "AlchemyRecipe", menuName = "Items/Alchemy/AlchemyRecipe")]
    public class AlchemyRecipe : ScriptableObject
    {
        public string RecipeID { get; set; }
        public string RecipeName { get; set; }
        
        public ItemData[] RequiredIngredients { get; set; }
        
        // 产出物品及数量
        public string OutputItemID { get; set; }
        public ItemData OutputItem { get; set; }
        public int OutputQuantity { get; set; }
    
        public float CraftingTime { get; set; }
    }
}