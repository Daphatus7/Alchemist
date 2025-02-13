// Author : Peiyu Wang @ Daphatus
// 10 02 2025 02 28

using System;
using System.Collections.Generic;
using _Script.Map;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.Quest.PlayerQuest;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Quest.GuildQuestUI
{
    public class GuildQuestUI : MonoBehaviour, IUIHandler, IGuildQuestUIHandler
    {
        public GameObject guildQuestDisplayPanel;
        public LayoutGroup questDisplayLayoutGroup;
        public GameObject questDisplayPrefab; 
        private readonly List<GuildQuestDisplay> _questDisplays = new List<GuildQuestDisplay>();
        private IGuildQuestGiverModuleHandler _handler;
        
        private void Awake()
        {
            ServiceLocator.Instance.Register<IGuildQuestUIHandler>(this);
            HideUI();
        }
        
        private void OnDestroy()
        {
            if (ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<IGuildQuestUIHandler>();
        }
        
        private void LoadQuestDisplays(List<GuildQuestDefinition> questDefinitions)
        {
            foreach (var o in _questDisplays)
            {
                Destroy(o.gameObject);
            }
            
            foreach (var questDefinition in questDefinitions)
            {
                AddQuestDisplay(questDefinition);
            }
        }
        
        private void AddQuestDisplay(GuildQuestDefinition questDefinition)
        {
            var questDisplay = Instantiate(questDisplayPrefab, questDisplayLayoutGroup.transform)
                .GetComponent<GuildQuestDisplay>();

            questDisplay.SetDisplay(
                questDefinition.questRank.ToString(),
                questDefinition.questName,
                questDefinition.description,
                questDefinition.ToString(),
                // Use a lambda so it is called on button click, not immediately
                () => OnQuestAcceptButtonClicked(questDefinition)
            );

            _questDisplays.Add(questDisplay);
        }

        private void OnQuestAcceptButtonClicked(GuildQuestDefinition questDefinition)
        {
            //Create the quest 
            if(QuestManager.Instance.CreateGuildQuest(questDefinition))
            {
                //Call the handler that hte quest has been accepted
                _handler.OnAcceptQuest(questDefinition);
                HideUI();
            }
            else
            {
                Debug.Log("Failed to create guild quest");
            }
        }

        public void ShowUI()
        {
            guildQuestDisplayPanel.SetActive(true);
        }

        public void LoadQuestGiver(IGuildQuestGiverModuleHandler handler)
        {
            ShowUI();
            _handler = handler;
            LoadQuestDisplays(handler.GetAvailableQuests);
        }

        public void HideUI()
        {
            guildQuestDisplayPanel.SetActive(false);
            _handler = null;
        }
    }
    
    public interface IGuildQuestUIHandler : IGameService
    {
        void LoadQuestGiver(IGuildQuestGiverModuleHandler handler);
        void HideUI();
    }
}