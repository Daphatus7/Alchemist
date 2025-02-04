// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 54

using System.Collections.Generic;

namespace _Script.Alchemy.RecipeBook
{
    public class PlayerRecipeBook
    {
        //Learned recipes
        private Dictionary<PotionType, List<AlchemyRecipe>> _recipes;
        
        public bool LearnRecipe(AlchemyRecipe recipe)
        {
            if (_recipes.ContainsKey(recipe.PotionType))
            {
                _recipes[recipe.PotionType].Add(recipe);
            }
            else
            {
                _recipes.Add(recipe.PotionType, new List<AlchemyRecipe>(){recipe});
            }
            return true;
        }
        
        public List<AlchemyRecipe> GetRecipesByType(PotionType type)
        {
            return _recipes[type];
        }
        
        public AlchemyRecipe GetRecipeByType(PotionType type, int index)
        {
            return _recipes[type][index];
        }
    }
}