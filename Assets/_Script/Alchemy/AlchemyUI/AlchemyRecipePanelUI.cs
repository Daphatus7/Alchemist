// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 19

using System.Collections.Generic;
using _Script.Inventory.SlotFrontend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{
    /// <summary>
    /// Display the recipe of the potion
    /// Potion info
    /// Description
    /// Reagents
    /// </summary>
    public class AlchemyRecipePanelUI : MonoBehaviour
    {
        [Header("Output")]
        [SerializeField] private Image outputIcon;
        [SerializeField] private TextMeshProUGUI outputNameText;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [Header("Reagents")]
        [SerializeField] private LayoutGroup reagentPanel;
        [SerializeField] private GameObject reagentDisplayPrefab;
        private List<AlchemyReagentDisplayUI> _reagentDisplays = new List<AlchemyReagentDisplayUI>();
        
        
        
        public void LoadRecipe(AlchemyRecipe recipe)
        {
            //Output
            outputIcon.sprite = recipe.mainOutputItem.itemIcon;
            outputNameText.text = recipe.mainOutputItem.itemName;
            descriptionText.text = recipe.mainOutputItem.itemDescription;
            
            //Reagents
            LoadReagents(recipe);
        }
        
        private void LoadReagents(AlchemyRecipe recipe)
        {
            _reagentDisplays.Clear();
            foreach (Transform child in reagentPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            foreach (var reagent in recipe.reagents)
            {
                var display = Instantiate(reagentDisplayPrefab, reagentPanel.transform).GetComponent<AlchemyReagentDisplayUI>();
                display.SetDisplay(reagent.Data.itemIcon, reagent.Data.itemName, reagent.Quantity);
                _reagentDisplays.Add(display);
            }
        }
        private void GetPlayerInventoryQuantity()
        {
            //TODO
        }
    }
}