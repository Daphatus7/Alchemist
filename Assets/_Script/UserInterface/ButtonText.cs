// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 59

using TMPro;
using UnityEngine;

namespace _Script.UserInterface
{
    public class ButtonText : MonoBehaviour
    {
        public TextMeshProUGUI buttonText;
        
        public void SetText(string text)
        {
            buttonText.text = text;
        }
    }
}