// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 53

using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Alchemy
{
    public class BrewInstance
    {
        private AlchemyRecipe _recipe;
        private List<ItemAndQuantity> _outputItems;
        
        //holds the recipe
        public BrewInstance(AlchemyRecipe recipe)
        {
            _recipe = recipe;
        }
        
        public List<ItemAndQuantity> GetOutputItems
        {
            get
            {
                if (_outputItems == null)
                {
                    _outputItems = new List<ItemAndQuantity>();
                    _outputItems.Add(new ItemAndQuantity(_recipe.mainOutputItem, _recipe.outputQuantity));
                    foreach (var byproduct in _recipe.secondaryOutputItems)
                    {
                        //floor the chance
                        var count = (int) Random.Range(0, byproduct.chance);
                        _outputItems.Add(new ItemAndQuantity(byproduct.item, count));
                    }
                    return _outputItems;
                }
                else
                {
                    return _outputItems;
                }
            }
        }
    }
}