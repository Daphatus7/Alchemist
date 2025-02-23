// Author : Peiyu Wang @ Daphatus
// 12 02 2025 02 50

using System;
using _Script.Character.PlayerRank;
using _Script.Quest.QuestDefinition;

namespace _Script.Quest.QuestInstance
{
    [Serializable]
    public class GuildQuestInstance : QuestInstance
    {

        public GuildQuestDefinition GuildQuestDefinition => (GuildQuestDefinition) QuestDefinition;
        private NiRank _questRank;
        public NiRank QuestRank => _questRank;

        public GuildQuestInstance(GuildQuestDefinition def, NiRank questRank) : base(def)
        {
            QuestState = QuestState.NotStarted;
            _questRank = questRank;
        }
        
        public GuildQuestInstance(GuildQuestDefinition def, GuildQuestSave save) : base(def, save)
        {
            _questRank = save.questRank;
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
        
        public override QuestSave OnSave()
        {
            var newSave = new GuildQuestSave
            {
                questId = QuestDefinition.questID,
                questState = QuestState,
                questRank = _questRank,
                objectives = new QuestObjectiveSave[objectives.Count],

            };
            for (int i = 0; i < objectives.Count; i++)
            {
                newSave.objectives[i] = objectives[i].OnSave(i);
            }
            return newSave;
        }
    }
    
    public class GuildQuestSave : QuestSave
    {
        public NiRank questRank;
    }
}
    
