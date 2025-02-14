// Author : Peiyu Wang @ Daphatus
// 12 02 2025 02 50

namespace _Script.Quest
{
    public class GuildQuestInstance : QuestInstance
    {

        public GuildQuestDefinition GuildQuestDefinition => (GuildQuestDefinition) QuestDefinition;
        public GuildQuestInstance(GuildQuestDefinition def) : base(def)
        {
            QuestState = QuestState.InProgress;
        }

        private bool _initialized = false;

        private int _distanceToTravel;

        public int DistanceToTravel
        {
            get
            {
                if (_initialized) return _distanceToTravel;
                _distanceToTravel =
                    UnityEngine.Random.Range(GuildQuestDefinition.distanceMin, GuildQuestDefinition.distanceMax);
                _initialized = true;
                return _distanceToTravel;
            }
        }

        public override QuestType QuestType => QuestType.Guild;
    }
}