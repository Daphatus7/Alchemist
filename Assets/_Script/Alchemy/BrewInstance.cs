// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 53

using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.ItemInstance;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Alchemy
{
    public class BrewInstance
    {
        //Target Inventory
        Inventory.InventoryBackend.Inventory _targetInventory;
        
        
        private AlchemyRecipe _recipe;
        private List<ItemAndQuantity> _outputItems;
        
        public float BrewTime => _recipe.craftingTime;
        
        //holds the recipe
        public BrewInstance(AlchemyRecipe recipe, 
            Inventory.InventoryBackend.Inventory targetInventory)
        {
            _targetInventory = targetInventory;
            _recipe = recipe;
        }
        
        public void CompleteBrew()
        {
            //add the output items to the target inventory
            foreach (var item in GetOutputItems)
            {
                _targetInventory.AddItem(ItemInstanceFactory.CreateItemInstance(item.Data, false, item.Quantity));
            }
        }
        
        
        public List<ItemAndQuantity> GetOutputItems
        {
            get
            {
                if (_outputItems == null)
                {
                    _outputItems = new List<ItemAndQuantity>
                    {
                        new ItemAndQuantity(_recipe.mainOutputItem, _recipe.outputQuantity)
                    };
                    foreach (var byproduct in _recipe.secondaryOutputItems)
                    {
                        //floor the chance
                        var count = 1;
                        _outputItems.Add(new ItemAndQuantity(byproduct.item, count));
                    }
                }
                return _outputItems;
            }
        }
    }
}