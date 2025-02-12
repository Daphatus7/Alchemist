// Author : Peiyu Wang @ Daphatus
// 12 02 2025 02 50

namespace _Script.Quest
{
    public class GuildQuestInstance : QuestInstance
    {

        public GuildQuestDefinition GuildQuestDefinition => (GuildQuestDefinition) QuestDefinition;
        public GuildQuestInstance(GuildQuestDefinition def) : base(def)
        {
        }

        public override QuestType QuestType => QuestType.Guild;
    }
}