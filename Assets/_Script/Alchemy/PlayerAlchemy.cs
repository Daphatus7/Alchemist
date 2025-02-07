// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 56

using System;
using _Script.Alchemy.RecipeBook;
using UnityEngine;

namespace _Script.Alchemy
{
    public class PlayerAlchemy : MonoBehaviour
    {
        private PlayerRecipeBook recipeBook; public PlayerRecipeBook RecipeBook => recipeBook;
        [SerializeField] AlchemyRecipe [] learnedRecipes;
        
        private Inventory.InventoryBackend.Inventory _playerInventory;
        private Inventory.InventoryBackend.Inventory PlayerInventory
        {
            get
            {
                if (_playerInventory == null)
                {
                    Debug.Log("你还没有初始化玩家的背包");
                    throw new NullReferenceException("Player Inventory is null");
                }
                else
                {
                    return _playerInventory;
                }
            }
        }


        public void Awake()
        {
            //Initialize the recipe book from the learned recipes
            recipeBook = new PlayerRecipeBook(learnedRecipes);
        }

        /// <summary>
        /// 通过PotionType和inventoryIndex获取配方
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inventoryIndex"></param>
        /// <returns></returns>
        public AlchemyRecipe GetRecipe(PotionCategory type, int inventoryIndex)
        {
            return recipeBook.GetRecipeByType(type, inventoryIndex);
        }
        
        /// <summary>
        /// 能否酿造
        /// 主要用于UI 显示，所以不需要太仔细
        /// </summary>
        /// <param name="recipe"></param>
        /// <returns></returns>
        public bool CanBrew(AlchemyRecipe recipe)
        {
            //check inventory status to see if the player has enough reagents
            foreach (var reagent in recipe.reagents)
            {
                if(PlayerInventory.GetItemCount(reagent.Data.itemID) < reagent.Quantity)
                {
                    return false;
                }
            }
            return false;
        }
        
        public bool CheckPlayerRealtimeInventory(AlchemyRecipe recipe)
        {
            if (_playerInventory == null)
            {
                Debug.Log("你还没有初始化玩家的背包");
                throw new NullReferenceException("Player Inventory is null");
            }
            
            foreach (var reagent in recipe.reagents)
            {
                if(PlayerInventory.CheckRealtimeItemCount(reagent.Data.itemID, reagent.Quantity))
                {
                    return false;
                }
            }
            return true;
        }
        
        public bool RemoveReagentsFromPlayerInventory(AlchemyRecipe recipe)
        {
            if (_playerInventory == null)
            {
                Debug.Log("你还没有初始化玩家的背包");
                throw new NullReferenceException("Player Inventory is null");
            }
            
            foreach (var reagent in recipe.reagents)
            {
                if(PlayerInventory.RemoveItemById(reagent.Data.itemID, reagent.Quantity))
                {
                    return false;
                }
            }
            return true;
        }
    }
}