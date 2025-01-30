// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 40

using _Script.NPC.NpcBackend;
using _Script.Quest;
using _Script.Quest.PlayerQuest;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
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

        private INpcQuestModuleHandler _currentNpc;
        
        public void LoadQuestData(INpcQuestModuleHandler handler)
        {
            _currentNpc = handler;
            questDescriptionText.text = handler.CurrentQuest.questDescription;
            questRewardText.text = handler.CurrentQuest.reward.ToString();
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
            ServiceLocator.Instance.Get<IPlayerQuestService>().AddNewSideQuest(new MainQuestInstance(_currentNpc.CurrentQuest));
            //load next dialogue
            
        }
        
        private void OnDeclineButtonClicked()
        {
            Debug.Log("Decline button clicked");
            //load next dialogue
        }
    }
}