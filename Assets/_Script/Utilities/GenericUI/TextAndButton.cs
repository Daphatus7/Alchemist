// Author : Peiyu Wang @ Daphatus
// 13 02 2025 02 15

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Utilities.GenericUI
{
    public class TextAndButton : MonoBehaviour
    {
        [SerializeField] private Button confirmButton;
        [SerializeField] private TextMeshProUGUI textDisplay;
        
        public void LoadUIContent(string text, Action confirmAction)
        {
            confirmButton.onClick.AddListener(() =>
            {
                confirmAction?.Invoke();
                gameObject.SetActive(false);
            });
            
            if (textDisplay != null)
            {
                textDisplay.text = text;
            }
        }    
    }
}