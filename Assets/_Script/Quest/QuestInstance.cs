// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 43

using System.Collections.Generic;

using System.Collections.Generic;
using UnityEngine;

namespace _Script.Quest
{
    public abstract class QuestInstance
    {
        private readonly QuestDefinition _definition;
        private readonly List<QuestObjective> _objectives = new List<QuestObjective>();

        public IReadOnlyList<QuestObjective> Objectives => _objectives;
        public QuestDefinition Definition => _definition;

        public QuestInstance(QuestDefinition def)
        {
            _definition = def;

            // For each static ObjectiveData, create a dynamic QuestObjective
            foreach (var objData in _definition.objectives)
            {
                var questObj = new QuestObjective(objData.objectiveData);
                _objectives.Add(questObj);
            }
        }
    }
}
