// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 36

using System;
using _Script.NPC.NpcBackend;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
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
                if (moduleHandler != null && moduleHandler.ShouldLoadModule())
                {
                    AddChoice(moduleHandler.ModuleInfo.ModuleName, () => HandleChoice(moduleHandler));
                }
            }
        }

        private void AddChoice(string choiceText, Action onClickAction)
        {
            var choice = Instantiate(choicePrefab, choicePanel.transform);

            var buttonText = choice.GetComponent<ButtonText>();
            if (buttonText != null)
            {
                buttonText.SetText(choiceText);
            }
            else
            {
                Debug.LogWarning("ButtonText component is missing in choicePrefab!");
            }

            var button = choice.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => onClickAction?.Invoke());
            }
            else
            {
                Debug.LogWarning("Button component is missing in choicePrefab!");
            }
        }
        
        
        /// <summary>
        /// When player click on the choice, the module will be loaded
        /// </summary>
        /// <param name="moduleHandler"></param>

        private void HandleChoice(INpcModuleHandler moduleHandler)
        {
            if (moduleHandler == null)
            {
                Debug.LogWarning("moduleHandler is null! Cannot load NPC module.");
                return;
            }
            moduleHandler.LoadNpcModule();
        }
    }
}
