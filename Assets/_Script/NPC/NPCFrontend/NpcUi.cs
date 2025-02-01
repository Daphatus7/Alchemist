// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 02

using System;
using System.Collections.Generic;
using _Script.NPC.NpcBackend;
using _Script.Quest;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.NPC.NPCFrontend
{
    public class NpcUi : NpcUiBase, INpcUIService, INpcUiCallback
    {
        #region Main UI Elements - main dialogue box
        [Header("UI Elements")] [Tooltip("Panel for the dialogue box")] [SerializeField]
        private GameObject dialogueOptionsButtonPrefab;

        [SerializeField] private GameObject dialogueModulePanel;
        
        [SerializeField] private Button closeButton;

        [Tooltip("Image for the pixel-style background")] [SerializeField]
        private Image backgroundImage;
        
        #endregion
        
        #region NpcUis - Choice, QuestGiver

        private Dictionary<NpcUiType, NpcUiBase> _npcUis;
        
        [SerializeField] private NpcChoiceUi npcChoiceUi;
        [SerializeField] private QuestGiverUi questGiverUi;
        
        #endregion

        #region Logic

        /// <summary>
        /// This allows the dialogue to be ended from the outside
        /// </summary>
        private event Action OnDialogueEnd;
        
        /// <summary>
        /// Npc Handler that is currently being interacted with
        /// </summary>
        private INpcDialogueHandler _currentDialogueHandler;

        #endregion
        
        
        protected void Awake()
        {
            closeButton.onClick.AddListener(EndDialogue);

            _npcUis = new Dictionary<NpcUiType, NpcUiBase>
            {
                { NpcUiType.Choice, npcChoiceUi },
                { NpcUiType.QuestGiver, questGiverUi }
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
            ServiceLocator.Instance.Register<INpcUiCallback>(this);
        }
        
        private void OnDisable()
        {
            // Check if ServiceLocator still exists
            if (ServiceLocator.Instance == null) return;
            
            ServiceLocator.Instance.Unregister<INpcUIService>();
            ServiceLocator.Instance.Unregister<INpcUiCallback>();
        }


        /// <summary>
        /// List all options listed by the NPC
        /// </summary>
        /// <param name="dialogueHandler"></param>
        public void StartDialogue(INpcDialogueHandler dialogueHandler)
        {
            ShowUI();

            //reference the current dialogue handler
            _currentDialogueHandler = dialogueHandler;
            
            //load the NPC text with choices
            var mainNpc = _currentDialogueHandler.GetNpcDialogue();
            var moduleHandlers = _currentDialogueHandler.GetAddonModules();
            
            //Load the NPC text
            LoadNpcChoice(mainNpc, moduleHandlers);
        }

        public void OnNpcUiChange(NpcUiType uiType)
        {
            throw new NotImplementedException();
        }

        public void LoadQuestUi(INpcQuestModuleHandler quest)
        {
            DisplayUi(NpcUiType.QuestGiver);
            questGiverUi.LoadQuestData(quest);
        }

        private void EndDialogue()
        {
            //就在这里，需要
            _currentDialogueHandler.OnDialogueEnd();
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


        protected virtual void OnOnDialogueEnd()
        {
            OnDialogueEnd?.Invoke();
        }
    }
    public enum NpcUiType
    {
        
        Main,
        Choice,
        QuestGiver
    }
    

}