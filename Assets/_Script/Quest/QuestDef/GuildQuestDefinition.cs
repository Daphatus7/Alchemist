// Author : Peiyu Wang @ Daphatus
// 09 02 2025 02 05

using _Script.Character.PlayerRank;
using UnityEngine;

namespace _Script.Quest.QuestDef
{
    [CreateAssetMenu(fileName = "GuildQuestDefinition", menuName = "Quests/GuildQuestDefinition")]
    public class GuildQuestDefinition : SimpleQuestDefinition
    {
        public NiRank questRank;
        public string description;
        
        [Header("Total Rooms")]
        public int distanceMax;
        public int distanceMin;
        //guild quest can only have 1 objective
        public ObjectiveType ObjectiveType => objectives[0].objectiveData.Type; 
    }
}