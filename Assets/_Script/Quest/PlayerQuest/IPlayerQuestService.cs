// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 46

using _Script.Utilities.ServiceLocator;

namespace _Script.Quest.PlayerQuest
{
    public interface IPlayerQuestService : IGameService
    {
        void AddNewSideQuest(QuestInstance quest);
        void CompleteQuest(QuestInstance quest);
        void AddNewGuildQuest(GuildQuestInstance quest);
    }
}