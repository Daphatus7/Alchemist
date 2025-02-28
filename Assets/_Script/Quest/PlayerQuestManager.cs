// Author : Peiyu Wang @ Daphatus
// 23 02 2025 02 56

using System;
using System.Collections.Generic;
using _Script.Quest.QuestInstance;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Quest
{
    public class PlayerQuestManager : MonoBehaviour, IPlayerQuestService
    {
        private readonly Dictionary<string, QuestInstance.QuestInstance> _activeQuests = new Dictionary<string, QuestInstance.QuestInstance>(); 
        
        //Quests are sorted automatically instead of using separate lists
        
        private GuildQuestInstance _activeGuildQuest;
        
        public void Update()
        {
            //Temporary only for testing
            if (!Prototype_Active_Quest_Ui.Instance) return;
            Prototype_Active_Quest_Ui.Instance.SetText("");
            string t = "";
            foreach (var quest in _activeQuests)
            {
                t += quest.Value.QuestStatus + "\n";
            } 
            t += "-----------------\n";
            if (_activeGuildQuest != null)
            {
                t += "Guild Quest: \n" + _activeGuildQuest.QuestStatus;
            }
            Prototype_Active_Quest_Ui.Instance.SetText(t);
        }
        
        public void OnEnable()
        {
            ServiceLocator.Instance?.Register<IPlayerQuestService>(this);
        }

        public void OnDisable()
        {
            if (!Application.isPlaying) return;

            // Check if ServiceLocator still exists
            if (ServiceLocator.Instance == null) return;

            var playerQuestService = ServiceLocator.Instance.Get<IPlayerQuestService>();
            if ((PlayerQuestManager)playerQuestService == this)
            {
                ServiceLocator.Instance.Unregister<IPlayerQuestService>();
            }
        }
        
        public void AddGuildQuest(QuestInstance.QuestInstance quest)
        {
            //check if there are any active guild quests

            if (_activeQuests.TryAdd(quest.QuestDefinition.questID, quest))
            {
                Debug.Log("Added quest: " + quest.QuestDefinition.questName);
            }
            else
            {
                Debug.Log("Failed to add quest: " + quest.QuestDefinition.questName);
            }
        }
        
        public void RemoveGuildQuest(QuestInstance.QuestInstance quest)
        {
            Debug.Log("Removing quest: " + quest.QuestDefinition.questID);
            _activeQuests.Remove(quest.QuestDefinition.questID);
        }
    }
    
    public interface IPlayerQuestService : IGameService
    {
        void AddGuildQuest(QuestInstance.QuestInstance quest);
        void RemoveGuildQuest(QuestInstance.QuestInstance quest);
    }
}