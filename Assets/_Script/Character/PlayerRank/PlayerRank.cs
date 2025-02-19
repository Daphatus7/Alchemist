// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 17

using System;
using UnityEngine;

namespace _Script.Character.PlayerRank
{
    public interface IPlayerRankHandler
    {
        void AddExperience(int exp);
        public NiRank CurrentRank { get; }
    }
    
    public class PlayerRank: IPlayerRankHandler
    {
        private PlayerRankState _currentRank;
        private int _totalExp = 0;
        private int _currentLevelExp = 0;

        // Expose current rank to other classes
        public NiRank CurrentRank => _currentRank.Rank;
        
        public event Action<float> onExperienceChanged;
        
        private void OnExperienceChanged()
        {
            onExperienceChanged?.Invoke((float)_currentLevelExp / GetNextState().ExpRequired);
        }
        
        // Initialize the state machine with the starting rank
        public PlayerRank()
        {
            // Start with the lowest rank (F)
            _currentRank = new PlayerRankF(this);
            _currentRank.Enter();
            _currentLevelExp = 0;
        }
        
        /// <summary>
        /// Call this method to add experience.
        /// It will check if the new total meets the threshold to upgrade to the next rank.
        /// </summary>
        public void AddExperience(int exp)
        {
            _totalExp += exp;
            _currentLevelExp += exp;
            // While we have a next state available and the total experience is enough, upgrade!
            var nextState = GetNextState();
            while (nextState != null && _totalExp >= nextState.ExpRequired)
            {
                ChangeState(nextState);
                nextState = GetNextState();
            }
            OnExperienceChanged();
        }

        /// <summary>
        /// Helper method that returns the next rank state, based on the current rank.
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
                    // Max rank reached – no next state.
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
            _currentLevelExp = 0;
            _currentRank.Enter();
        }
    }


    
}