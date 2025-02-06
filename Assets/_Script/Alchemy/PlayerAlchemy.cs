// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 56

using _Script.Alchemy.RecipeBook;
using UnityEngine;

namespace _Script.Alchemy
{
    public class PlayerAlchemy : MonoBehaviour
    {
        /// <summary>
        /// Get recipe
        /// </summary>
        private PlayerRecipeBook _recipeBook; 
        public PlayerRecipeBook RecipeBook => _recipeBook;
        
        
        
        private Inventory.InventoryBackend.Inventory _playerInventory;
        
        /// <summary>
        /// 通过PotionType和inventoryIndex获取配方
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inventoryIndex"></param>
        /// <returns></returns>
        public AlchemyRecipe GetRecipe(PotionCategory type, int inventoryIndex)
        {
            return _recipeBook.GetRecipeByType(type, inventoryIndex);
        }
        
        public bool Brew(AlchemyRecipe recipe)
        {
            //create a new brew instance
            //start the timer
            //after the timer is done
            //check if the player has the ingredients
            //if the player has the ingredients
            //remove the ingredients
            //add the output items
            return false;
        }
    }
}