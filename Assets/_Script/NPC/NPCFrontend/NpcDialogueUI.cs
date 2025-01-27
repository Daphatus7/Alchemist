// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 02

using System;
using _Script.NPC.NpcBackend;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NPCFrontend
{
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace _Script.NPC.NPCFrontend
{
    public class NpcDialogueUI : MonoBehaviour, IUIHandler, INpcUIService
    {
        [Header("UI Elements")]
        [Tooltip("Panel for the dialogue box")]
        public GameObject dialoguePanel;

        [Tooltip("Text field for displaying the dialogue")]
        public TextMeshProUGUI dialogueText;

        [Tooltip("Image for the pixel-style background")]
        public Image backgroundImage;

        [Tooltip("Next button to proceed in the dialogue")]
        public Button nextButton;

        [Header("Dialogue Settings")]
        [Tooltip("Typing speed for dialogue text")]
        public float typingSpeed = 0.05f;

        private string[] dialogues; // List of dialogues to display
        private int currentDialogueIndex; // Tracks the current dialogue
        private bool isTyping; // Tracks if text is currently typing

        public event Action OnDialogueEnd;
        
        private void Start()
        {
            // Hide the dialogue panel initially
            ServiceLocator.Instance.Register<INpcUIService>(this);
            HideUI();
        }

        public void OnEnable()
        {
            if (nextButton != null)
            {
                nextButton.onClick.AddListener(DisplayNextDialogue);
            }
        }
        
        public void OnDisable()
        {
            if (nextButton != null)
            {
                nextButton.onClick.RemoveListener(DisplayNextDialogue);
            }
        }
        


        private void DisplayDialogue(string dialogue)
        {
            if (isTyping)
            {
                StopAllCoroutines(); // Stop current typing coroutine
                dialogueText.text = dialogue; // Display full text immediately
                isTyping = false;
                return;
            }
            
            StartCoroutine(TypeDialogue(dialogue));
        }

        private System.Collections.IEnumerator TypeDialogue(string dialogue)
        {
            isTyping = true;
            dialogueText.text = "";

            foreach (char letter in dialogue)
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(typingSpeed);
            }

            isTyping = false;
        }

        private void DisplayNextDialogue()
        {
            if (isTyping) return;

            currentDialogueIndex++;
            if (currentDialogueIndex < dialogues.Length)
            {
                DisplayDialogue(dialogues[currentDialogueIndex]);
            }
            else
            {
                EndDialogue();
            }
        }

        private void EndDialogue()
        {
            OnDialogueEnd?.Invoke();
            HideUI();
        }

        public void ShowUI()
        {
            dialoguePanel.SetActive(true);
        }

        public void HideUI()
        {
            dialoguePanel.SetActive(false);
        }

        
        public void StartDialogue(INpcDialogueHandler dialogueHandler)
        {
            
        }
        
        public void StartDialogue(string[] npcDialogues)
        {
            dialogues = npcDialogues;
            currentDialogueIndex = 0;
            ShowUI();
            DisplayDialogue(dialogues[currentDialogueIndex]);
        }
    }
}

}