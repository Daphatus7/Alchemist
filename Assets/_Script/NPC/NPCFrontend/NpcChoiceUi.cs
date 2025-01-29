// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 36

using System;
using _Script.NPC.NpcBackend;
using _Script.NPC.NpcBackend.NpcModules;
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

        private void ClearChoices()
        {
            foreach (Transform child in choicePanel.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public void LoadNpcChoice(NpcInfo mainNpc, INpcModuleHandler[] moduleHandlers)
        {
            ClearChoices();
            npcDialogueText.text = mainNpc.NpcDialogue;

            foreach (var moduleHandler in moduleHandlers)
            {
                AddChoice(moduleHandler.ModuleInfo.ModuleName, () => HandleChoice(moduleHandler));
            }
        }

        private void AddChoice(string choiceText, Action onClickAction)
        {
            var choice = Instantiate(choicePrefab, choicePanel.transform);
            choice.GetComponentInChildren<ButtonText>().SetText(choiceText);
            choice.GetComponent<Button>().onClick.AddListener(() => onClickAction?.Invoke());
        }

        private void HandleChoice(INpcModuleHandler moduleHandler)
        {
            switch (moduleHandler.HandlerType)
            {
                case NpcHandlerType.Merchant:
                    LoadMerchantPanel(moduleHandler);
                    break;
                case NpcHandlerType.QuestGiver:
                    LoadQuestGiverPanel(moduleHandler);
                    break;
                default:
                    Debug.Log($"Unhandled NPC Module: {moduleHandler.ModuleInfo.ModuleName}");
                    break;
            }
        }

        public void LoadMerchantPanel(INpcModuleHandler npcMerchant)
        {

        }

        public void LoadQuestGiverPanel(INpcModuleHandler npcQuestGiver)
        {
            Debug.Log("Loading Quest Giver Panel...");
            // Implement quest giver-specific logic here
        }
    }
}
