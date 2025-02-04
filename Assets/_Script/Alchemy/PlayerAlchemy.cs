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
        
        /// <summary>
        /// Progress Bar
        /// </summary>
        private BrewInstance _brewInstance;
        
        /// <summary>
        /// Used for store the items
        /// </summary>
        private Inventory.InventoryBackend.Inventory _inventory;
        
        
        public AlchemyRecipe GetRecipe(PotionType type, int inventoryIndex)
        {
            return _recipeBook.GetRecipeByType(type, inventoryIndex);
        }
    }
}