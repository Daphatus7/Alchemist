// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 54

using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Alchemy.RecipeBook
{
    [Serializable]
    public class PlayerRecipeBook
    {
        //Learned recipes
        private Dictionary<PotionCategory, List<AlchemyRecipe>> _recipes;
        
        public Dictionary<PotionCategory, List<AlchemyRecipe>> Recipes => _recipes;
        
        public PlayerRecipeBook(AlchemyRecipe[] learnedRecipes)
        {
            _recipes = new Dictionary<PotionCategory, List<AlchemyRecipe>>();
            foreach (var learned in learnedRecipes)
            {
                var category = learned.PotionCategory;
                if (_recipes.ContainsKey(category))
                {
                    _recipes[category].Add(learned);
                }
                else
                {
                    _recipes.Add(category, new List<AlchemyRecipe>(){learned});
                }
            }
        }
        
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