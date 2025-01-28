// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 36

using System;
using System.Collections.Generic;
using _Script.NPC.NpcBackend;
using _Script.UserInterface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.NPC.NPCFrontend
{
    public class NpcChoiceUi : NpcUiBase
    {
        [SerializeField] private LayoutGroup choicePanel;
        [SerializeField] private GameObject choicePrefab;
        [SerializeField] private TextMeshProUGUI npcDialogueText;
        
        public event Action<int> OnChoiceSelected;
        
        public void AddChoice(string choiceText, int choiceIndex)
        {
            var choice = Instantiate(choicePrefab, choicePanel.transform);
            choice.GetComponentInChildren<Text>().text = choiceText;
            choice.GetComponent<Button>().onClick.AddListener(() => OnChoiceSelected?.Invoke(choiceIndex));
        }

        private void ClearChoices()
        {
            foreach (Transform child in choicePanel.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void LoadNpcChoice(NpcInfo mainNpc, INpcModuleHandler [] moduleHandlers)
        {
            ClearChoices();
            
            npcDialogueText.text = mainNpc.NpcDialogue;
            foreach (var moduleHandler in moduleHandlers)
            {
                var questModule = moduleHandler;
                if (questModule != null)
                {
                    AddChoice(questModule.ModuleInfo.ModuleName, (int) questModule.HandlerType);
                }
            }
        }
        
        public void LoadMerchantPanel(INpcModuleHandler npcMerchant)
        {

        }
        
        public void LoadQuestGiverPanel(INpcModuleHandler npcQuestGiver)
        {
            
        }
        
        
    }
}