// Author : Peiyu Wang @ Daphatus
// 30 01 2025 01 10

using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Quest
{
    /// <summary>
    /// Represents the major story segments or "chapters"
    /// in your game (Tutorial, Quest1, etc.).
    /// </summary>
    public enum MainStoryLine
    {
        Tutorial,
        Quest1,
        Quest2,
    }

    /// <summary>
    /// Serializable class to store a single storyline entry
    /// with a boolean completion state.
    /// </summary>
    [Serializable]
    public class StorylineProgress
    {
        public MainStoryLine storyline;
        public bool isCompleted;
    }
    

    /// <summary>
    /// A ScriptableObject that maintains a fast O(1) dictionary
    /// to track each major storyline's completion state.
    /// 
    /// Because Unity can't directly serialize dictionaries,
    /// we use a List for the inspector and synchronize
    /// it with a Dictionary at runtime.
    /// </summary>
    [CreateAssetMenu(fileName = "StorylineChecker", menuName = "Quests/Storyline Checker")]
    public class StorylineChecker : ScriptableObject
    {
        [Header("Serialized Data (for Inspector)")]
        [SerializeField]
        private List<StorylineProgress> serializedProgress = new List<StorylineProgress>();

        /// <summary>
        /// Runtime dictionary for O(1) lookups:
        /// Key = MainStoryLine, Value = completion state.
        /// </summary>
        private Dictionary<MainStoryLine, bool> _progressMap;

        /// <summary>
        /// Called automatically when the ScriptableObject
        /// is first loaded or reloaded. We'll build our
        /// dictionary from the serialized list here.
        /// </summary>
        private void OnEnable()
        {
            BuildDictionaryFromList();
        }

        /// <summary>
        /// Rebuilds the runtime dictionary from the
        /// serialized list, ensuring O(1) lookups at runtime.
        /// </summary>
        private void BuildDictionaryFromList()
        {
            _progressMap = new Dictionary<MainStoryLine, bool>();

            foreach (var entry in serializedProgress)
            {
                // Ensure no duplicates or handle them as needed
                if (!_progressMap.ContainsKey(entry.storyline))
                {
                    _progressMap.Add(entry.storyline, entry.isCompleted);
                }
            }
        }

        /// <summary>
        /// Is the specified storyline completed?
        /// O(1) lookup.
        /// </summary>
        public bool IsCompleted(MainStoryLine storyline)
        {
            // If dictionary is not yet built for some reason (e.g. called at runtime),
            // safeguard by building it.
            if (_progressMap == null)
                BuildDictionaryFromList();

            return _progressMap.TryGetValue(storyline, out bool result) && result;
        }

        /// <summary>
        /// Marks a storyline as completed or not
        /// </summary>
        public void SetCompleted(MainStoryLine storyline, bool completed)
        {
            if (_progressMap == null)
                BuildDictionaryFromList();

            // Update the dictionary in O(1)
            _progressMap[storyline] = completed;

            // Also keep the serialized list in sync so that
            // Inspector data is up-to-date.
            UpdateListEntry(storyline, completed);
        }

        /// <summary>
        /// Updates the serialized list so it remains consistent
        /// with the dictionary changes. This way, if you look at
        /// the ScriptableObject in the Inspector, it reflects
        /// the current runtime state.
        /// </summary>
        private void UpdateListEntry(MainStoryLine storyline, bool completed)
        {
            // Find existing entry
            var entry = serializedProgress.Find(x => x.storyline == storyline);
            if (entry == null)
            {
                // Create a new list entry
                entry = new StorylineProgress
                {
                    storyline = storyline,
                    isCompleted = completed
                };
                serializedProgress.Add(entry);
            }
            else
            {
                entry.isCompleted = completed;
            }
        }

        /// <summary>
        /// Resets every storyline to false (not completed).
        /// Useful for debugging, new game starts, etc.
        /// </summary>
        public void ResetAllProgress()
        {
            // Clear dictionary and list
            if (_progressMap != null)
                _progressMap.Clear();
            serializedProgress.Clear();
        }
    }
}
