// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 40

using _Script.Quest;
using _Script.UserInterface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.NPC.NPCFrontend
{
    public class QuestGiverUi : NpcUiBase
    {
        [SerializeField] private Button acceptButton;
        [SerializeField] private Button declineButton;
        
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private TextMeshProUGUI questRewardText;
        
        
        public void LoadQuestData(QuestDefinition quest)
        {
            questDescriptionText.text = quest.questDescription;
            questRewardText.text = quest.reward.ToString();
        }
        
        
        public override void ShowUI()
        {
            base.ShowUI();
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            declineButton.onClick.AddListener(OnDeclineButtonClicked);
        }
        
        public override void HideUI()
        {
            base.HideUI();
            acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);
            declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
        }
        
        private void OnAcceptButtonClicked()
        {
            Debug.Log("Accept button clicked");
        }
        
        private void OnDeclineButtonClicked()
        {
            Debug.Log("Decline button clicked");
        }
    }
}