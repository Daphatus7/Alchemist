// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 42

using System.Collections.Generic;

namespace _Script.Quest.PlayerQuest
{
    public class PlayerQuestManager : IPlayerQuestService
    {
        private List<QuestInstance> _activeQuests; public List<QuestInstance> ActiveQuests => _activeQuests;
        private List<QuestInstance> _mainQuests; public List<QuestInstance> MainQuests => _mainQuests;
        private Queue<QuestInstance> _completedQuests; public Queue<QuestInstance> CompletedQuests => _completedQuests;
        
        public void AddNewQuest(QuestInstance quest)
        {
            _activeQuests.Add(quest);
        }
        
        public void AddMainQuest(QuestInstance quest)
        {
            _mainQuests.Add(quest);
        }
        
    }
}