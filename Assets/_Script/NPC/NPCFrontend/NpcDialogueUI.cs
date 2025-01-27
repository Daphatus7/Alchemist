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
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private GameObject dialogueOptionsPanel;
        [SerializeField] private GameObject dialogueOptionsButtonPrefab;
        
        [Tooltip("Text field for displaying the dialogue")]
        [SerializeField] private TextMeshProUGUI dialogueText;

        [Tooltip("Image for the pixel-style background")]
        [SerializeField] private Image backgroundImage;
        
        

        [Header("Dialogue Settings")]
        [Tooltip("Typing speed for dialogue text")]
        [SerializeField] private float typingSpeed = 0.05f;
        
        
        //--------------------------------------------------------------------------------
        
        
        //--------------------------------------------------------------------------------
        
        
        //---------------------------------------Remove this-----------------------------------------
        private string[] dialogues; // 改成从NpcDialogueHandler里面获取
        private int currentDialogueIndex; // Tracks the current dialogue
        private bool isTyping; // Tracks if text is currently typing
        //--------------------------------------------------------------------------------
        private void Start()
        {
            // Hide the dialogue panel initially
            ServiceLocator.Instance.Register<INpcUIService>(this);
            HideUI();
        }
        
        
        private INpcDialogueHandler _currentDialogueHandler;
        
        public void StartDialogue(INpcDialogueHandler dialogueHandler)
        {
            _currentDialogueHandler = dialogueHandler;
            //Load the data from the dialogue handler
            
            //Load the NPC text
            
            
            ShowUI();
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
            //OnDialogueEnd?.Invoke();
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

        
        
        
  
    }
}

}