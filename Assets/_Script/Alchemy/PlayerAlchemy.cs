// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 56

using _Script.Alchemy.RecipeBook;
using UnityEngine;

namespace _Script.Alchemy
{
    public class PlayerAlchemy : MonoBehaviour
    {
        private PlayerRecipeBook _recipeBook;
        private BrewInstance _brewInstance;
        private Inventory.InventoryBackend.Inventory _inventory;
        
        
        public AlchemyRecipe GetRecipe(PotionType type, int inventoryIndex)
        {
            return _recipeBook.GetRecipeByType(type, inventoryIndex);
        }
    }
}