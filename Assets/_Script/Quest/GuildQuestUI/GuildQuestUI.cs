// Author : Peiyu Wang @ Daphatus
// 10 02 2025 02 28

using System;
using System.Collections.Generic;
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
                () => OnQuestAcceptButtonClicked(questDefinition.questID)
            );

            _questDisplays.Add(questDisplay);
        }

        private void OnQuestAcceptButtonClicked(string questId)
        {
            //add quest to player quest list
            //Create a new quest object and add it to the player quest list
            HideUI();
        }

        public void ShowUI()
        {
            guildQuestDisplayPanel.SetActive(true);
        }

        public void LoadQuests(List<GuildQuestDefinition> questDefinitions)
        {
            ShowUI();
            LoadQuestDisplays(questDefinitions);
        }

        public void HideUI()
        {
            guildQuestDisplayPanel.SetActive(false);
        }
    }
    
    public interface IGuildQuestUIHandler : IGameService
    {
        void LoadQuests(List<GuildQuestDefinition> questDefinitions);
        void HideUI();
    }
}