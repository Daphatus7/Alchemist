// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 30

using System;
using _Script.Alchemy.AlchemyTools;
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
        [Header("Backend")]
        private PlayerAlchemy _playerAlchemy; 
        private Inventory.InventoryBackend.Inventory _playerContainer;
        private AlchemyRecipe _selectedRecipe;
        private AlchemyTool _alchemyTool;
        
        
        [Header("Frontend")]
        [SerializeField] private GameObject alchemyInventoryPanel;
        [SerializeField] private AlchemyRecipesUI alchemyRecipesUI;
        [SerializeField] private AlchemyRecipePanelUI alchemyRecipePanelUI;
        [FormerlySerializedAs("cauldronContainerUI")] [SerializeField] private AlchemyContainerUI alchemyContainerUI;
        
        [SerializeField] private Button brewButton;
        
        public void Awake()
        {
            alchemyInventoryPanel.SetActive(false);
        }
        
        public void OnEnable()
        {
            ServiceLocator.Instance.Register<IAlchemyUIService>(this);
            brewButton.onClick.AddListener(BrewButton);
        }
        
        public void OnDisable()
        {

            brewButton.onClick.RemoveListener(BrewButton);
            
            if(ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<IAlchemyUIService>();
        }

        public void ShowUI()
        {
            if(_playerAlchemy == null)
            {
                throw new NullReferenceException("Player Alchemy is null");
            }
            alchemyInventoryPanel.SetActive(true);
            alchemyContainerUI.ShowUI();
        }

        public void HideUI()
        {
            //Unsubscribe
            alchemyRecipesUI.onRecipeSelected -= OnRecipeSelected;
            alchemyInventoryPanel.SetActive(false);
            
            //清空数据
            _selectedRecipe = null;
            _alchemyTool = null;
            _playerAlchemy = null;
        }
        
        /// <summary>
        /// 加载左边的所有配方
        /// </summary>
        private void LoadPlayerAlchemy(PlayerAlchemy playerAlchemy, Inventory.InventoryBackend.Inventory playerContainer)
        {
            alchemyRecipesUI.LoadPlayerAlchemy(playerAlchemy, playerContainer.InventoryStatus);
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
            if(_selectedRecipe != null //检查是否选中了配方
               || _alchemyTool != null  //检查是否有炼金台
               || !_alchemyTool.IsEmpty) //目前必须保证炼金台为空
            {
                //如果不能制作，比如材料不够, 简单检查数据
                if (!_playerAlchemy.CanBrew(_selectedRecipe))
                {
                    Debug.Log("材料不够");
                }
                else
                {
                    if(_playerAlchemy.CheckPlayerRealtimeInventory(_selectedRecipe))
                    {
                        Debug.Log("移除物品");
                        //remove reagents from the player inventory
                        _playerAlchemy.RemoveReagentsFromPlayerInventory(_selectedRecipe);
                        _alchemyTool.StartBrew(new BrewInstance(_selectedRecipe, _playerContainer));
                        _alchemyTool.onBrewComplete += OnBrewComplete;
                    }
                }
            }
        }
        private void OnBrewComplete()
        {
            Debug.Log("Brew Complete, Play UI");
            _alchemyTool.onBrewComplete -= OnBrewComplete;
        }

        public void LoadRecipe(AlchemyRecipe recipe)
        {
            _selectedRecipe = recipe;
            alchemyRecipePanelUI.LoadRecipe(recipe);
        }


        /// <summary>
        /// 当玩家与炼金台交互时加载UI
        /// 步骤，
        /// 1. 加载锅里的物品
        /// 2. 加载玩家的配方
        /// 3. 加载默认选中的配方
        /// </summary>
        /// <param name="player"></param>
        /// <param name="playerInventory"></param>
        /// <param name="alchemyTool"></param>
        public void LoadAlchemyUI(PlayerAlchemy player, //玩家
            Inventory.InventoryBackend.Inventory playerInventory, //玩家背包，用来
            AlchemyTool alchemyTool //炼金台
            )
        {
            _playerAlchemy = player;
            _alchemyTool = alchemyTool;
            _playerContainer = playerInventory;
            
            //加载
            LoadPlayerAlchemy(_playerAlchemy, _playerContainer);
            
            //加载Container
            alchemyContainerUI.LoadContainer(_alchemyTool.Container);
            
            //当选中配方，在配方界面加载对应的配方
            alchemyRecipesUI.onRecipeSelected += OnRecipeSelected;
            ShowUI();
        }
    }
}