// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 45

using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.UserInterface;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{

    /// <summary>
    /// holds the item selection and tab changing
    /// </summary>
    public class AlchemyRecipesUI : MonoBehaviour, IUIHandler
    {
        
        /// <summary>
        /// Title such as flask, potion 
        /// </summary>
        [SerializeField] private TextMeshProUGUI titleText;
        
        //Holds recipe displays
        [SerializeField] private LayoutGroup recipePanel;

        /// <summary>
        /// Display buttons on the page
        /// </summary>
        [SerializeField] private GameObject recipeDisplayPrefab;

        /// <summary>
        /// tabs, recipes
        /// </summary>
        private List<List<AlchemyRecipeDisplayUI>> _recipeDisplays = new List<List<AlchemyRecipeDisplayUI>>();
        
        /// <summary>
        /// Tabs
        /// </summary>
        [SerializeField] private GameObject tabPrefab;
        [SerializeField] private LayoutGroup tabPanel;
        
        //Tabs
        private readonly List<GameObject> _tabs = new List<GameObject>();
        private GameObject _activeTab;
        
        /// <summary>
        /// Tab selected, recipe selected
        /// </summary>
        public event Action<Tuple<PotionCategory,int>> onRecipeSelected;
        
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="playerAlchemy"></param>
        /// <exception cref="NullReferenceException"></exception>
        public void LoadPlayerAlchemy(PlayerAlchemy playerAlchemy, 
            InventoryStatus playerInventory //用于检查材料
            )
        {
            //Load the tabs
            if (playerAlchemy.RecipeBook.Recipes.Count > 0)
            {
                LoadTabs(playerAlchemy);
                LoadTabOfRecipes(playerAlchemy.RecipeBook.GetRecipesByType(playerAlchemy.RecipeBook.Recipes.Keys.GetEnumerator().Current));
            }
            else
            {
                throw new NullReferenceException("No recipes found");
            }
        }
        
        /// <summary>
        /// 加载所有的tabs
        /// </summary>
        /// <param name="playerAlchemy"></param>
        private void LoadTabs(PlayerAlchemy playerAlchemy)
        {
            //Clear the current tabs
            foreach (Transform child in tabPanel.transform)
            {
                Destroy(child.gameObject);
            }
            var tabs = playerAlchemy.RecipeBook.Recipes.Keys;
            foreach (var type in tabs)
            {
                AddTabButton(type.ToString());
            }
        }
        
        /// <summary>
        /// 加载所有tabs的recipes
        /// </summary>
        /// <param name="recipes"></param>
        private void LoadTabOfRecipes(List<AlchemyRecipe> recipes)
        {
            _recipeDisplays.Clear();   
            //Clear the current displays
            foreach (Transform child in recipePanel.transform)
            {
                Destroy(child.gameObject);
            }
            //Get the recipes for the tab
            
            //显示该类别中的所有物品
            foreach (var recipe in recipes)
            {
                AddRecipeDisplay(recipe, recipes.IndexOf(recipe));
            }
        }
        
        
        private void OnRecipeSelected(PotionCategory type, int index)
        {
            onRecipeSelected?.Invoke(new Tuple<PotionCategory, int>(type, index));
        }
        
        
        /// <summary>
        /// Add tabs
        /// </summary>
        /// <param name="tabName"></param>
        private void AddTabButton(string tabName)
        {
            var tab = Instantiate(tabPrefab, tabPanel.transform);
            var buttonText = tab.GetComponent<ButtonText>();
            buttonText.SetText(tabName);
            var index = _tabs.Count;
            tab.GetComponent<Button>().onClick.AddListener(() => OnTabClicked(index));
            _tabs.Add(tab);
            //Load the recipes for the tab
        }
        
        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="recipe"></param>
        /// <param name="recipeIndex"></param>
        private void AddRecipeDisplay(AlchemyRecipe recipe, int recipeIndex)
        {
            var recipeDisplay = Instantiate(recipeDisplayPrefab, recipePanel.transform);
            var display = recipeDisplay.GetComponent<AlchemyRecipeDisplayUI>();
            display.SetDisplay(recipe.mainOutputItem.itemIcon, recipe.mainOutputItem.itemName);
            
            //Bind Interaction 
            recipeDisplay.GetComponent<Button>().onClick.AddListener(() => OnRecipeSelected(recipe.PotionCategory, recipeIndex));
        }
        
        private void OnTabClicked(int index)
        {
            Debug.Log("Tab clicked" + index);
        }

        public void ShowUI()
        {
            throw new System.NotImplementedException();
        }

        public void HideUI()
        {
            throw new System.NotImplementedException();
        }
    }
}