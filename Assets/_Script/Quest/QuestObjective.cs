// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 46

using System;
using System.Collections.Generic;
using _Script.Enemy.EnemyData;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Quest
{
    [Serializable]
    public class QuestObjective
    {
        // Polymorphic reference to handle different objective types
        [SerializeReference] // Allows referencing derived classes
        public ObjectiveData objectiveData;
        public bool isComplete;
        private int _currentCount;
        public int CurrentCount { get; set;}
        
        // Create a runtime objective from static data
        public QuestObjective(ObjectiveData data)
        {
            objectiveData = data;
            CurrentCount = 0;
            isComplete = false;
        }
    }
    [Serializable]
    public abstract class ObjectiveData
    {
        public abstract ObjectiveType Type { get; }
        public int requiredCount;
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
        public EnemyData enemy; // The enemy to kill
        public override ObjectiveType Type => ObjectiveType.Kill;
    }

// Add more specific objective types as needed
    [Serializable]
    public class ExplorationObjective : ObjectiveData
    {
        public string location; // Example: Area name or coordinates
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