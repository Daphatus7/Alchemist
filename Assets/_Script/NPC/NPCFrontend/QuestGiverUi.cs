// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 40

using System;
using System.Collections;
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
        [SerializeField] private Button nextDialogueButton;
        [SerializeField] private float typingSpeed = 0.05f;

        
        
        private INpcQuestModuleHandler _currentNpc;
        
        // Dialogue typing-related variables
        private string[] _dialogues;            // Array of current dialogue lines
        private int _currentDialogueIndex;      // Tracks which line we’re on
        private bool _isTyping;                 // True if we are mid-typing characters

        
        
        public void LoadQuestData(INpcQuestModuleHandler handler)
        {
            _currentNpc = handler;
            
            if(handler.CurrentQuest == null)
            {
                //this should not happen
                throw new Exception("Quest is null");
            }
            
            var quest = handler.CurrentQuest.QuestDefinition;
            if (quest == null)
            {
                throw new Exception("QuestDefinition is null");
            }
            
            // Show correct dialogue based on quest state
            switch (handler.CurrentQuest.QuestState)
            {
                case QuestState.NotStarted:
                    StartNpcDialogue(handler.CurrentQuest.QuestDefinition.questStartDialogue);
                    break;
                
                case QuestState.InProgress:
                    StartNpcDialogue(handler.CurrentQuest.QuestDefinition.questInProgressDialogue);
                    break;
                
                case QuestState.Completed:
                    StartNpcDialogue(handler.CurrentQuest.QuestDefinition.questCompleteDialogue);
                    break;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        
        /// <summary>
        /// Begins the typed dialogue sequence using the given NpcDialogue data.
        /// </summary>
        private void StartNpcDialogue(NpcDialogue npcDialogue)
        {
            // Fall back if no dialogue is provided
            if (npcDialogue?.dialogue == null || npcDialogue.dialogue.Length == 0)
            {
                // Could display some default message or skip
                _dialogues = new[] { NpcDialogue.defaultDialogue };
            }
            else
            {
                _dialogues = npcDialogue.dialogue;
            }

            _currentDialogueIndex = 0;
            
            // Display the first line of dialogue
            DisplayDialogueLine(_dialogues[_currentDialogueIndex]);
        }
        
        /// <summary>
        /// Displays a given dialogue line. If currently typing, it will stop and show it immediately.
        /// Otherwise, it types it out character by character.
        /// </summary>
        private void DisplayDialogueLine(string line)
        {
            if (_isTyping)
            {
                // If user clicks 'Next' while text is still typing, skip to the end
                StopAllCoroutines();
                questDescriptionText.text = line;
                _isTyping = false;
            }
            else
            {
                // Start the typing coroutine
                StartCoroutine(TypeDialogue(line));
            }
        }
        
        /// <summary>
        /// Types out the given string character by character, at the configured typing speed.
        /// </summary>
        private IEnumerator TypeDialogue(string line)
        {
            _isTyping = true;
            questDescriptionText.text = string.Empty;

            foreach (char letter in line.ToCharArray())
            {
                questDescriptionText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            _isTyping = false;
        }
        
        /// <summary>
        /// Called when the user clicks the "Next Dialogue" button.
        /// Displays the next line if available; otherwise ends the dialogue.
        /// </summary>
        private void OnNextDialogueClicked()
        {
            if (_isTyping)
            {
                // If text is still typing, skip to the end
                DisplayDialogueLine(_dialogues[_currentDialogueIndex]);
                return;
            }

            _currentDialogueIndex++;
            if (_dialogues != null && _currentDialogueIndex < _dialogues.Length)
            {
                DisplayDialogueLine(_dialogues[_currentDialogueIndex]);
            }
            else
            {
                EndDialogue();
            }
        }
        
        /// <summary>
        /// End of dialogue handling. You can hide the panel or trigger further logic here.
        /// </summary>
        private void EndDialogue()
        {
            // You could fire an event, or simply hide the panel.
            Debug.Log("Dialogue ended.");
            
            // Optionally, you might want to do some logic like auto-closing the UI 
            // or giving control back to the player, etc.
        }

        #region Show / Hide UI

        public override void ShowUI()
        {
            base.ShowUI();
    
            if (acceptButton == null)
            {
                Debug.LogError("acceptButton is null in ShowUI");
            }
            else
            {
                acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            }

            if (declineButton == null)
            {
                Debug.LogError("declineButton is null in ShowUI");
            }
            else
            {
                declineButton.onClick.AddListener(OnDeclineButtonClicked);
            }

            if (nextDialogueButton == null)
            {
                Debug.LogError("nextDialogueButton is null in ShowUI");
            }
            else
            {
                nextDialogueButton.onClick.AddListener(OnNextDialogueClicked);
            }
        }

        public override void HideUI()
        {
            base.HideUI();

            if (!acceptButton)
            {
                Debug.LogError("acceptButton is null in HideUI");
            }
            else
            {
                acceptButton.onClick.RemoveListener(OnAcceptButtonClicked);
            }

            if (!declineButton)
            {
                Debug.LogError("declineButton is null in HideUI");
            }
            else
            {
                declineButton.onClick.RemoveListener(OnDeclineButtonClicked);
            }

            if (!nextDialogueButton)
            {
                Debug.LogError("nextDialogueButton is null in HideUI");
            }
            else
            {
                nextDialogueButton.onClick.RemoveListener(OnNextDialogueClicked);
            }
        }

        #endregion
        
       
        
        private void OnAcceptButtonClicked()
        {
            ServiceLocator.Instance.Get<IPlayerQuestService>().AddNewSideQuest(_currentNpc.CurrentQuest);
            StartNpcDialogue(_currentNpc.CurrentQuest.QuestDefinition.questInProgressDialogue);
        }
        
        private void OnDeclineButtonClicked()
        {
            Debug.Log("Decline button clicked");
            //load next dialogue
        }
    }
}