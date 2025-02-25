// Author : Peiyu Wang @ Daphatus
// 09 02 2025 02 13

using System;
using UnityEngine;

namespace _Script.Character.PlayerRank
{
    public interface IPlayerRankHandler
    {
        void AddExperience(int exp);
        NiRank CurrentRank { get; }
        PlayerRankSave OnSave();
        void OnLoad(PlayerRankSave saveData);
    }
    
    [Serializable]
    public class PlayerRankSave
    {
        public NiRank rank;
        public int totalExp;
        public int currentLevelExp;
    }
    
    public class PlayerRank : IPlayerRankHandler
    {
        private PlayerRankState _currentRank;
        public int TotalExp { get; set; }
        public int CurrentLevelExp { get; set; }

        // Expose current rank to other classes
        public NiRank CurrentRank => _currentRank.Rank;
        
        public float Progress => (float)CurrentLevelExp / GetNextState().ExpRequired;
        public event Action<float> onExperienceChanged;
        
        private void OnExperienceChanged()
        {
            var nextState = GetNextState();
            // Avoid divide-by-zero if no next state exists
            float progress = nextState != null ? (float)CurrentLevelExp / nextState.ExpRequired : 1f;
            onExperienceChanged?.Invoke(progress);
        }
        
        // Initialize the state machine with the starting rank
        public PlayerRank()
        {
            // Start with the lowest rank (F)
            _currentRank = new PlayerRankF(this);
            _currentRank.Enter();
            CurrentLevelExp = 0;
        }
        
        /// <summary>
        /// Adds experience and automatically promotes the player rank if thresholds are met.
        /// </summary>
        public void AddExperience(int exp)
        {
            TotalExp += exp;
            CurrentLevelExp += exp;
            // While we have a next state available and the total experience is enough, upgrade!
            var nextState = GetNextState();
            while (nextState != null && TotalExp >= nextState.ExpRequired)
            {
                ChangeState(nextState);
                nextState = GetNextState();
            }
            OnExperienceChanged();
        }

        /// <summary>
        /// Returns the next rank state based on the current rank.
        /// </summary>
        private PlayerRankState GetNextState()
        {
            switch (_currentRank.Rank)
            {
                case NiRank.F:
                    return new PlayerRankE(this);
                case NiRank.E:
                    return new PlayerRankD(this);
                case NiRank.D:
                    return new PlayerRankC(this);
                case NiRank.C:
                    return new PlayerRankB(this);
                case NiRank.B:
                    // Highest rank reached – no next state.
                    return null;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Handles the transition from one rank state to the next.
        /// </summary>
        private void ChangeState(PlayerRankState newState)
        {
            _currentRank.Exit();
            _currentRank = newState;
            CurrentLevelExp = 0;
            _currentRank.Enter();
        }
        
        /// <summary>
        /// Generates a PlayerRankSave instance representing the current rank state.
        /// </summary>
        public PlayerRankSave OnSave()
        {
            return new PlayerRankSave
            {
                rank = _currentRank.Rank,
                totalExp = TotalExp,
                currentLevelExp = CurrentLevelExp
            };
        }
        
        /// <summary>
        /// Loads the player rank from the provided save data.
        /// This method recreates the proper state based on the saved rank, and restores exp values.
        /// </summary>
        public void OnLoad(PlayerRankSave saveData)
        {
            if (saveData == null)
            {
                Debug.LogWarning("PlayerRank.OnLoad: Save data is null.");
                return;
            }

            // Restore experience values.
            TotalExp = saveData.totalExp;
            CurrentLevelExp = saveData.currentLevelExp;

            // Recreate the state corresponding to the saved rank.
            switch (saveData.rank)
            {
                case NiRank.F:
                    _currentRank = new PlayerRankF(this);
                    break;
                case NiRank.E:
                    _currentRank = new PlayerRankE(this);
                    break;
                case NiRank.D:
                    _currentRank = new PlayerRankD(this);
                    break;
                case NiRank.C:
                    _currentRank = new PlayerRankC(this);
                    break;
                case NiRank.B:
                    _currentRank = new PlayerRankB(this);
                    break;
                default:
                    Debug.LogWarning("PlayerRank.OnLoad: Unknown rank. Defaulting to F.");
                    _currentRank = new PlayerRankF(this);
                    break;
            }
            
            // Enter the restored state.
            _currentRank.Enter();
            OnExperienceChanged();
        }
    }
}