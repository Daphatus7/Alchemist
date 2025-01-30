// Author : Peiyu Wang @ Daphatus
// 30 01 2025 01 27

using TMPro;
using UnityEngine;

namespace _Script.UserInterface
{
    public class Prototype_Active_Quest_Ui : Singleton<Prototype_Active_Quest_Ui>
    {
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        
        
        public void SetText(string text)
        {
            questDescriptionText.text = text;
        }
    }
}