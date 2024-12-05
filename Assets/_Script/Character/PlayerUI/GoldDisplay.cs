// Author : Peiyu Wang @ Daphatus
// 04 12 2024 12 35

using System;
using _Script.Managers;
using TMPro;
using UnityEngine;

namespace _Script.Character.PlayerUI
{
    
    [DefaultExecutionOrder(300)]
    public class GoldDisplay : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI goldText;
        
        private void Awake()
        {
            goldText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            SetGoldText(GameManager.Instance.GetPlayer().Gold);
            GameManager.Instance.GetPlayer().PlayerGoldUpdateEvent().AddListener(SetGoldText);
        }
        
        private void OnDestroy()
        {
            GameManager.Instance.GetPlayer().PlayerGoldUpdateEvent().RemoveListener(SetGoldText);
        }

        private void SetGoldText(int gold)
        {
            goldText.text = "Gold: " + gold;
        }
    }
}