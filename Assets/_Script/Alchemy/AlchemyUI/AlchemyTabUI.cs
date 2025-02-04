// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 45

using UnityEngine;
using UnityEngine.UI;

namespace _Script.Alchemy.AlchemyUI
{
    
    /// <summary>
    /// holds the item selection and tab changing
    /// </summary>
    public class AlchemyTabUI : MonoBehaviour
    {
        //Holds recipe displays
        [SerializeField] private LayoutGroup _layoutGroup;
        
        /// <summary>
        /// Display buttons on the page
        /// </summary>
        [SerializeField] private GameObject _recipeDisplayPrefab;
        
    }
}