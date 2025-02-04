// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 19

using _Script.Inventory.SlotFrontend;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{
    public class AlchemyRecipePanelUI : MonoBehaviour
    {
        [SerializeField] private InventorySlotDisplay resultIcon;
        [SerializeField] private TextMeshProUGUI descriptionText;
        
        [SerializeField] private LayoutGroup ingredientLayoutGroup;
        [SerializeField] private InventorySlotDisplay[] ingredientIcons;
        
        
        public void SetDisplay(AlchemyRecipe recipe)
        {
            resultIcon.SetDisplay(recipe.mainOutputItem, 1);
            descriptionText.text = recipe.mainOutputItem.itemDescription;
            for (int i = 0; i < recipe.ingredients.Length; i++)
            {
                ingredientIcons[i].SetDisplay(recipe.ingredients[i], 1);
            }
        }
    }
}