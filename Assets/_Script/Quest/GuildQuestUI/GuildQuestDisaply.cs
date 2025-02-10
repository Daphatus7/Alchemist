// Author : Peiyu Wang @ Daphatus
// 10 02 2025 02 20

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Quest.GuildQuestUI
{
    public class GuildQuestDisplay : MonoBehaviour
    {
        public TextMeshProUGUI rankText;
        public TextMeshProUGUI titleText;
        public TextMeshProUGUI descriptionText;
        public TextMeshProUGUI rewardsText;
        public Button acceptButton;
        
        public void SetDisplay(string rank, string title, string description, string rewards, Action acceptAction)
        {
            rankText.text = rank;
            titleText.text = title;
            descriptionText.text = description;
            rewardsText.text = rewards;

            acceptButton.onClick.AddListener(() =>
            {
                acceptAction?.Invoke();
            });
        }
    }
}