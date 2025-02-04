// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 30

using System;
using UnityEngine;

namespace _Script.Alchemy.AlchemyUI
{
    public class PlayerAlchemyUI : MonoBehaviour
    {
        private PlayerAlchemy _playerAlchemy;
        private PotionType _currentPotionTab;
        private AlchemyRecipe _selectedRecipe;
        
        
        private void LoadRecipeTabButton()
        {
            //Load the recipe tab
        }
        
        private void LoadRecipes()
        {
            //Load the recipes
        }
        
        //Select the recipe to brew
        public void SelectRecipe(int index)
        {
            var selected = _playerAlchemy.GetRecipe(_currentPotionTab, index);
            if(selected != null)
            {
                _selectedRecipe = selected;
                LoadRecipe(selected);
            }
            else
            {
                throw new NullReferenceException("Recipe not found");
            }
        }
        
        
        // To brew the potion selected
        public void BrewButton()
        {
            
        }
        
        public void LoadRecipeTab(PotionType type)
        {
            _currentPotionTab = type;
        }
        
        public void LoadRecipe(AlchemyRecipe recipe)
        {
            _selectedRecipe = recipe;
        }
    }
}