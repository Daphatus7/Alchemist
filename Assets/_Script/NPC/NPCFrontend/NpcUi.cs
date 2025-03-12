// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 02

using System;
using System.Collections.Generic;
using _Script.NPC.NpcBackend;
using _Script.Utilities.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.NPC.NPCFrontend
{
    public class NpcUi : NpcUiBase, INpcUIService
    {
        #region Main UI Elements - main dialogue box
        [Header("UI Elements")] [Tooltip("Panel for the dialogue box")] [SerializeField]
        private GameObject dialogueOptionsButtonPrefab;

        protected INpcDialogueHandler CurrentDialogueHandler;

        [SerializeField] private GameObject dialogueModulePanel;
        
        [SerializeField] private Button closeButton;

        [Tooltip("Image for the pixel-style background")] [SerializeField]
        private Image backgroundImage;
        
        #endregion
        
        #region NpcUis - Choice, QuestGiver

        private Dictionary<NpcUiType, NpcUiBase> _npcUis;
        
        [SerializeField] private NpcChoiceUi npcChoiceUi;
        
        #endregion

        #region Logic
        
        /// <summary>
        /// Npc Handler that is currently being interacted with
        /// </summary>

        #endregion
        
        
        protected void Awake()
        {
            closeButton.onClick.AddListener(TerminateDialogue);

            _npcUis = new Dictionary<NpcUiType, NpcUiBase>
            {
                { NpcUiType.Choice, npcChoiceUi },
            };
        }

        private void Start()
        {
            // Hide the dialogue panel initially
            HideUI();
        }

        private void OnEnable()
        {
            ServiceLocator.Instance.Register<INpcUIService>(this);
        }
        
        private void OnDisable()
        {
            // Check if ServiceLocator still exists
            if (ServiceLocator.Instance == null) return;
            
            ServiceLocator.Instance.Unregister<INpcUIService>();
        }


        /// <summary>
        /// List all options listed by the NPC
        /// </summary>
        /// <param name="dialogueHandler"></param>
        public void StartDialogue(INpcDialogueHandler dialogueHandler)
        {
            ShowUI();

            //reference the current dialogue handler
            CurrentDialogueHandler = dialogueHandler;
            
            //load the NPC text with choices
            var mainNpc = CurrentDialogueHandler.GetNpcDialogue();
            var moduleHandlers = CurrentDialogueHandler.GetAddonModules();
            
            //Load the NPC text
            LoadNpcChoice(mainNpc, moduleHandlers);
        }
        

        public void TerminateDialogue()
        {
            CurrentDialogueHandler.TerminateConversation();
            HideUI();
        }

        private void DisplayUi(NpcUiType uiType)
        {
            foreach (var npcUi in _npcUis)
            {
                npcUi.Value.HideUI();
            }
            if(_npcUis.ContainsKey(uiType))
                _npcUis[uiType].ShowUI();
        }

        private void LoadNpcChoice(NpcInfo mainNpc, INpcModuleHandler [] moduleHandlers)
        {
            DisplayUi(NpcUiType.Choice);
            npcChoiceUi.LoadNpcChoice(mainNpc, moduleHandlers);
        }
        
    }
    public enum NpcUiType
    {
        
        Main,
        Choice,
        QuestGiver
    }
    

}