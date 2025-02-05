// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 54

using System.Collections.Generic;

namespace _Script.Alchemy.RecipeBook
{
    public class PlayerRecipeBook
    {
        //Learned recipes
        private Dictionary<PotionCategory, List<AlchemyRecipe>> _recipes;
        
        public Dictionary<PotionCategory, List<AlchemyRecipe>> Recipes => _recipes;
        
        public bool LearnRecipe(AlchemyRecipe recipe)
        {
            if (_recipes.ContainsKey(recipe.PotionCategory))
            {
                _recipes[recipe.PotionCategory].Add(recipe);
            }
            else
            {
                _recipes.Add(recipe.PotionCategory, new List<AlchemyRecipe>(){recipe});
            }
            return true;
        }
        
        public List<AlchemyRecipe> GetRecipesByType(PotionCategory type)
        {
            return _recipes[type];
        }
        
        public AlchemyRecipe GetRecipeByType(PotionCategory type, int index)
        {
            return _recipes[type][index];
        }
    }
}