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
        public event Action OnDialogueEnd;
        
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
            ServiceLocator.Instance.Unregister<INpcUIService>();
            ServiceLocator.Instance.Unregister<INpcUiCallback>();
        }


        public void StartDialogue(INpcDialogueHandler dialogueHandler)
        {
            _currentDialogueHandler = dialogueHandler;
            //Load the data from the dialogue handler
            
            //load the NPC text with choices
            var mainNpc = _currentDialogueHandler.GetNpcDialogue();
            var moduleHandlers = _currentDialogueHandler.GetAddonModules();
            LoadNpcChoice(mainNpc, moduleHandlers);
            //Load the NPC text
            ShowUI();
        }

        public void OnNpcUiChange(NpcUiType uiType)
        {
            throw new NotImplementedException();
        }

        public void LoadQuestUi(QuestDefinition quest)
        {
            DisplayUi(NpcUiType.QuestGiver);
            questGiverUi.LoadQuestData(quest);
        }

        private void EndDialogue()
        {
            OnDialogueEnd?.Invoke();
            HideUI();
        }
        
        public void DisplayUi(NpcUiType uiType)
        {
            foreach (var npcUi in _npcUis)
            {
                npcUi.Value.HideUI();
            }
            if(_npcUis.ContainsKey(uiType))
                _npcUis[uiType].ShowUI();
        }
        
        public void LoadNpcChoice(NpcInfo mainNpc, INpcModuleHandler [] moduleHandlers)
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