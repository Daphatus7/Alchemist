// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 46

using System;
using System.Collections.Generic;
using _Script.Enemy.EnemyData;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Quest.QuestDefinition
{
    /// <summary>
    /// Use both for instance and definition
    /// </summary>
    [Serializable]
    public class QuestObjective
    {
        // Polymorphic reference to handle different objective types
        [SerializeReference] // Allows referencing derived classes
        public ObjectiveData objectiveData;
        public bool isComplete;
        private int _currentCount;
        public int CurrentCount { get; set; }

        // Create a runtime objective from static data
        public QuestObjective(ObjectiveData data)
        {
            objectiveData = data;
            CurrentCount = 0;
            isComplete = false;
        }
        
        public virtual QuestObjectiveSave OnSave(int index)
        {
            var save = new QuestObjectiveSave
            {
                isComplete = isComplete,
                currentCount = CurrentCount
            };
            return save;
        }
        
        /// <summary>
        /// Unpack the save data
        /// </summary>
        /// <param name="save"></param>
        /// <returns></returns>
        public virtual void OnLoad(QuestObjectiveSave save)
        {
            isComplete = save.isComplete;
            CurrentCount = save.currentCount;
        }
    }

    [Serializable]
    public class QuestObjectiveSave
    {
        //check the objective data
        //can be obtained from the quest scriptable object
        /// <summary>
        /// The index of the objective in the quest
        /// </summary>
        public bool isComplete;
        public int currentCount;
    }
    
    [Serializable]
    public abstract class ObjectiveData
    {
        public abstract ObjectiveType Type { get; }
        public int requiredCount = 1;
    }

    [Serializable]
    public class CollectObjective : ObjectiveData
    {
        public ItemData item; // The item to collect
        public override ObjectiveType Type => ObjectiveType.Collect;
    }

    [Serializable]
    public class KillObjective : ObjectiveData
    {
        public EnemyData enemy; // The enemy to kill, assigned manually in editor
        [ReadOnly, ShowInInspector]
        private string _enemyID;
        public string EnemyID
        {
            get
            {
                if (enemy != null)
                {
                    _enemyID = enemy.enemyID;
                }
                return _enemyID;
            }
            set
            {
                enemy = DatabaseManager.Instance.GetEnemyPrefab(value).GetComponent<EnemyData>();
                _enemyID = value;
            }
        }
        public override ObjectiveType Type => ObjectiveType.Kill;
    }
    
    [Serializable]
    public class BossKillObjective : KillObjective
    {
        public string mapName;
    }
    
    [Serializable]
    public class MapCollectObjective : CollectObjective
    {
        public string mapName;
    }

// Add more specific objective types as needed
    [Serializable]
    public class ExplorationObjective : ObjectiveData
    {
        public string areaID; // Example: Area name or coordinates
        public bool isExplored;
        public override ObjectiveType Type => ObjectiveType.Explore;
    }

    public enum ObjectiveType
    {
        Kill,
        Collect,
        Explore
    }
    
    [Serializable]
    public class QuestReward
    {
        public int gold;
        public int experience;
        //Item IDs and amounts
        public RewardPair [] items;

        public override string ToString()
        {
            string allItems = "";
            foreach (var item in items)
            {
                allItems += $"{item.item.itemName} x{item.amount}\n";
            }
            return $"Gold: {gold}\n" +
                   $"Experience: {experience}\n" +
                   $"Items: = {allItems}\n";
        }
    }
    
    [Serializable]
    public class RewardPair
    {
        public ItemData item;
        public int amount;
    }
    
}