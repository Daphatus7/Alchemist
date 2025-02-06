// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 30

using System;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{
    public class PlayerAlchemyUI : MonoBehaviour, IUIHandler, IAlchemyUIService
    {
        private PlayerAlchemy _playerAlchemy; 
        private AlchemyRecipe _selectedRecipe;
        
        [SerializeField] private AlchemyRecipesUI alchemyRecipesUI;
        [SerializeField] private AlchemyRecipePanelUI alchemyRecipePanelUI;
        [SerializeField] private CauldronContainerUI cauldronContainerUI;

        public void Start()
        {
            ServiceLocator.Instance.Register<IAlchemyUIService>(this);
        }
        
        public void OnDestroy()
        {
            ServiceLocator.Instance.Unregister<IAlchemyUIService>();
        }

        public void ShowUI()
        {
            if(_playerAlchemy == null)
            {
                throw new NullReferenceException("Player Alchemy is null");
            }
            LoadPlayerAlchemy();
            alchemyRecipesUI.onRecipeSelected += OnRecipeSelected;
        }

        public void HideUI()
        {
            //Unsubscribe
            alchemyRecipesUI.onRecipeSelected -= OnRecipeSelected;
        }
        
        /// <summary>
        /// 加载左边的所有配方
        /// </summary>
        private void LoadPlayerAlchemy()
        {
            alchemyRecipesUI.LoadPlayerAlchemy(_playerAlchemy);
            //加载默认的配方
            OnRecipeSelected(new Tuple<PotionCategory, int>(PotionCategory.Potion, 0));
        }
        
        //Select the recipe to brew
        private void OnRecipeSelected(Tuple<PotionCategory,int> index)
        {
            //get the recipe by potion type and index
            var selected = _playerAlchemy.GetRecipe(index.Item1, index.Item2);
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
            if(_selectedRecipe != null)
            {
                if (_playerAlchemy.Brew(_selectedRecipe))
                {
                    //Update UI to show the new inventory
                }
                else
                {
                    throw new NullReferenceException("Brew failed");
                }
            }
        }
        
        public void LoadRecipe(AlchemyRecipe recipe)
        {
            _selectedRecipe = recipe;
        }


        /// <summary>
        /// 当玩家与炼金台交互时加载UI
        /// 步骤，
        /// 1. 加载锅里的物品
        /// 2. 加载玩家的配方
        /// 3. 加载默认选中的配方
        /// </summary>
        /// <param name="player"></param>
        /// <param name="cauldron"></param>
        public void LoadAlchemyUI(PlayerAlchemy player, Cauldron.Cauldron cauldron)
        {
            _playerAlchemy = player;
        }
    }
}