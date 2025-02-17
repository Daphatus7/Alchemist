// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 40

using System;
using System.Collections;
using _Script.NPC.NpcBackend;
using _Script.Quest;
using _Script.Quest.PlayerQuest;
using _Script.Quest.QuestDef;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // for UnityAction

namespace _Script.NPC.NPCFrontend
{
    public class QuestGiverUi : NpcUiBase
    {
        [Header("Button Prefab Settings")]
        [Tooltip("Prefab that will be instantiated for each button needed.")]
        [SerializeField] private GameObject ButtonPrefab;

        [Tooltip("Parent panel where spawned buttons will be placed.")]
        [SerializeField] private GameObject buttonPanel;
        
        [Header("Text Fields")]
        [SerializeField] private TextMeshProUGUI questDescriptionText;
        [SerializeField] private TextMeshProUGUI questRewardText;

        [Header("Dialogue Typing")]
        [SerializeField] private float typingSpeed = 0.05f;

        private INpcQuestModuleHandler _currentNpc;
        
        // Dialogue typing-related variables
        private string[] _dialogues;           // Array of current dialogue lines
        private int _currentDialogueIndex;     // Tracks which line we’re on
        private bool _isTyping;                // True if we are mid-typing characters

        // Keep track of the current quest state to handle button logic
        private QuestState _currentQuestState;

        /// <summary>
        /// True if we've typed out (or skipped) through all lines in _dialogues.
        /// </summary>
        private bool IsDialogueFinished
        {
            get
            {
                // Dialogue is "finished" if we're not typing AND we've already shown the last line.
                return !_isTyping && _currentDialogueIndex >= _dialogues.Length - 1;
            }
        }

        public void LoadQuestData(INpcQuestModuleHandler handler)
        {
            _currentNpc = handler;
            if (handler.CurrentAvailableQuest == null)
                throw new Exception("Quest is null");

            var quest = handler.CurrentAvailableQuest;
            if (quest == null)
                throw new Exception("QuestDefinition is null");

            // Optional: Show quest details
            questDescriptionText.text = quest.questName;
            questRewardText.text = quest.reward != null
                ? $"Reward: {quest.reward}"
                : "No Rewards";

            // If there's a CurrentQuest in progress, use its state; 
            // otherwise assume NotStarted
            _currentQuestState = handler.CurrentQuest?.QuestState ?? QuestState.NotStarted;
            
            // Pick dialogue based on quest state
            switch (_currentQuestState)
            {
                case QuestState.NotStarted:
                    StartNpcDialogue(quest.questStartDialogue);
                    break;

                case QuestState.InProgress:
                    StartNpcDialogue(quest.questInProgressDialogue);
                    break;

                case QuestState.Completed:
                    StartNpcDialogue(quest.questCompleteDialogue);
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
            if (npcDialogue == null || npcDialogue.dialogue == null || npcDialogue.dialogue.Length == 0)
            {
                _dialogues = new[] { NpcDialogue.defaultDialogue };
            }
            else
            {
                _dialogues = npcDialogue.dialogue;
            }

            _currentDialogueIndex = 0;

            // Display the first line of dialogue
            DisplayDialogueLine(_dialogues[_currentDialogueIndex]);

            // Create or remove relevant buttons based on current state
            ConfigureButtonsForState(_currentQuestState);
        }

        /// <summary>
        /// Decides which buttons to show based on the quest state and 
        /// whether the dialogue has finished.
        /// </summary>
        private void ConfigureButtonsForState(QuestState state)
        {
            // Clear out old buttons each time we re-configure
            ClearButtonPanel();

            switch (state)
            {
                case QuestState.NotStarted:
                    if (IsDialogueFinished)
                    {
                        // If we've typed through all lines, show "Accept" and "Decline"
                        CreateButton("Accept", OnAcceptButtonClicked);
                        CreateButton("Decline", OnDeclineButtonClicked);
                    }
                    else
                    {
                        // If dialogue is NOT yet finished, show "Next" to proceed
                        CreateButton("Next", OnNextDialogueClicked);
                    }
                    break;

                case QuestState.InProgress:
                    // In this scenario, no buttons are displayed.
                    // The player is out doing the quest tasks.
                    break;

                case QuestState.Completed:
                    // Show a "Complete Quest" button
                    CreateButton("Complete Quest", OnCompleteQuestClicked);
                    break;
            }
        }

        /// <summary>
        /// Spawns a button from the ButtonPrefab, sets its text to 'label',
        /// and attaches the given click action.
        /// </summary>
        private void CreateButton(string label, UnityAction onClickAction)
        {
            if (ButtonPrefab == null || buttonPanel == null)
            {
                Debug.LogError("ButtonPrefab or buttonPanel not assigned in inspector.");
                return;
            }

            // Instantiate the prefab
            GameObject newButtonObj = Instantiate(ButtonPrefab, buttonPanel.transform);

            // Try to get the Button component
            Button buttonComponent = newButtonObj.GetComponent<Button>();
            if (buttonComponent == null)
            {
                Debug.LogError("ButtonPrefab does not have a Button component!");
                return;
            }

            // Set the button text
            TextMeshProUGUI textComponent = newButtonObj.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.text = label;
            }
            else
            {
                Debug.LogWarning("No TextMeshProUGUI found in ButtonPrefab children.");
            }

            // Add the click listener
            buttonComponent.onClick.AddListener(onClickAction);
        }

        /// <summary>
        /// Removes all existing children (buttons) from the buttonPanel.
        /// </summary>
        private void ClearButtonPanel()
        {
            if (buttonPanel == null) return;

            // Destroy all child objects
            for (int i = buttonPanel.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = buttonPanel.transform.GetChild(i).gameObject;
                Destroy(child);
            }
        }

        #region Dialogue Methods

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

        #endregion

        #region Button Handlers

        /// <summary>
        /// Called when user clicks "Next" to proceed with the next line of dialogue.
        /// </summary>
        private void OnNextDialogueClicked()
        {
            if (_isTyping)
            {
                // If text is still typing, skip to the end
                DisplayDialogueLine(_dialogues[_currentDialogueIndex]);
                return;
            }

            // Move to the next line
            _currentDialogueIndex++;
            if (_currentDialogueIndex < _dialogues.Length)
            {
                DisplayDialogueLine(_dialogues[_currentDialogueIndex]);
            }

            // After displaying that line, check if we've reached the end
            if (IsDialogueFinished)
            {
                // Re-configure buttons so that we show Accept/Decline (for NotStarted) 
                // or do nothing if InProgress, etc.
                ConfigureButtonsForState(_currentQuestState);
            }
            else
            {
                // We still have lines left, so keep a "Next" button
                ConfigureButtonsForState(_currentQuestState);
            }
        }

        /// <summary>
        /// Called when user clicks "Accept" (after finishing all starting dialogue).
        /// </summary>
        private void OnAcceptButtonClicked()
        {
            Debug.Log("Accepting quest...");

            _currentNpc.StartQuest();
            ServiceLocator.Instance.Get<IPlayerQuestService>().AddNewSideQuest(_currentNpc.CurrentQuest);

            // Mark quest as InProgress
            _currentNpc.CurrentQuest.QuestState = QuestState.InProgress;
            _currentQuestState = QuestState.InProgress;

            // Clear buttons because we don't want any in-progress UI
            ConfigureButtonsForState(_currentQuestState);
        }

        /// <summary>
        /// Called when user clicks "Decline".
        /// </summary>
        private void OnDeclineButtonClicked()
        {
            Debug.Log("Decline button clicked.");
            HideUI();
        }

        /// <summary>
        /// Called if the user wants to complete the quest after it's in Completed state.
        /// </summary>
        private void OnCompleteQuestClicked()
        {
            Debug.Log("Completing quest...");
            // Mark quest as Completed - call [game manager or NPC ] to handle rewards, etc.
            if (_currentNpc.CompleteQuest())
            {
                Debug.Log("Quest completed successfully. + maybe update UI");
            }
            // Complete the quest Dialogue
            ServiceLocator.Instance.Get<INpcUIService>().TerminateDialogue();
            HideUI();
        }

        #endregion

        #region Show / Hide UI Overrides
        
        public override void HideUI()
        {
            base.HideUI();
            ClearButtonPanel();
        }

        #endregion
    }
}
