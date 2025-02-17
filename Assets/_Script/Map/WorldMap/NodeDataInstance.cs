// Author : Peiyu Wang @ Daphatus
// 14 02 2025 02 07

using _Script.Character.PlayerRank;
using _Script.Map.WorldMap.MapNode;
using _Script.Quest;

namespace _Script.Map.WorldMap
{
    public class NodeDataInstance
    {
        public PlayerRankEnum MapRank { get; protected set; }
        public NodeType NodeType { get; protected internal set; }
        public string Description { get; protected set; }
        public string MapName { get; protected set; }

        // Insert generation instruction
        
        public NodeDataInstance(
            string mapName,
            string description,
            NodeType nodeType,
            PlayerRankEnum mapRank)
        {
            MapRank = mapRank;
            MapName = mapName;
            NodeType = nodeType;
            Description = description;
        }
    }
    
    public abstract class QuestNodeInstance : NodeDataInstance
    {
        public abstract ObjectiveType ObjectiveType { get; }
        
        public QuestNodeInstance(
            string mapName, 
            string description, 
            NodeType nodeType, 
            PlayerRankEnum mapRank) : base(mapName, description, nodeType, mapRank)
        {
        }
    }
    
    public class BossNodeInstance : QuestNodeInstance
    {
        public string BossName { get; private set; }
        public BossNodeInstance(GuildQuestInstance questInstance, PlayerRankEnum mapRank)
            : base(
                ((BossKillObjective)questInstance.GuildQuestDefinition.objectives[0].objectiveData).mapName,
                questInstance.GuildQuestDefinition.description,
                NodeType.Boss,
                mapRank)
        {
            // Redundant assignments removed since base constructor already sets these values.
            BossName = ((BossKillObjective)questInstance.GuildQuestDefinition.objectives[0].objectiveData).enemyID;
        }

        public override ObjectiveType ObjectiveType => ObjectiveType.Kill;
    }
    
    public class CollectNodeInstance : QuestNodeInstance
    {
        public string ItemName { get; private set; }
        
        public CollectNodeInstance(GuildQuestInstance questInstance, PlayerRankEnum mapRank)
            : base(
                questInstance.GuildQuestDefinition.questName,
                questInstance.GuildQuestDefinition.description,
                NodeType.Resource,
                mapRank)
        {
            ItemName = ((CollectObjective)questInstance.GuildQuestDefinition.objectives[0].objectiveData).item.itemName;
        }
        
        public override ObjectiveType ObjectiveType => ObjectiveType.Collect;
    }
    
    public class ExploreNodeInstance : QuestNodeInstance
    {
        public string AreaName { get; private set; }
        
        public ExploreNodeInstance(GuildQuestInstance questInstance, PlayerRankEnum mapRank)
            : base(
                questInstance.GuildQuestDefinition.questName,
                questInstance.GuildQuestDefinition.description,
                NodeType.Resource,
                mapRank)
        {
            AreaName = ((ExplorationObjective)questInstance.GuildQuestDefinition.objectives[0].objectiveData).areaID;
        }
        
        public override ObjectiveType ObjectiveType => ObjectiveType.Explore;
    }
}