// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 17

using UnityEngine;

namespace _Script.Character.PlayerRank
{
    public interface IPlayerRankHandler
    {
        void AddExperience(int exp);
        public PlayerRankEnum CurrentRank { get; }
    }
    
    public class PlayerRank: IPlayerRankHandler
    {
        private PlayerRankState _currentRank;
        private int _totalExp = 0;

        // Expose current rank to other classes
        public PlayerRankEnum CurrentRank => _currentRank.Rank;

        // Initialize the state machine with the starting rank
        public PlayerRank()
        {
            // Start with the lowest rank (F)
            _currentRank = new PlayerRankF(this);
            _currentRank.Enter();
        }

        /// <summary>
        /// Call this method to add experience.
        /// It will check if the new total meets the threshold to upgrade to the next rank.
        /// </summary>
        public void AddExperience(int exp)
        {
            _totalExp += exp;
            Debug.Log("Added exp: " + exp + ", Total exp: " + _totalExp);

            // While we have a next state available and the total experience is enough, upgrade!
            PlayerRankState nextState = GetNextState();
            while (nextState != null && _totalExp >= nextState.ExpRequired)
            {
                ChangeState(nextState);
                nextState = GetNextState();
            }
        }

        /// <summary>
        /// Helper method that returns the next rank state, based on the current rank.
        /// </summary>
        private PlayerRankState GetNextState()
        {
            switch (_currentRank.Rank)
            {
                case PlayerRankEnum.F:
                    return new PlayerRankE(this);
                case PlayerRankEnum.E:
                    return new PlayerRankD(this);
                case PlayerRankEnum.D:
                    return new PlayerRankC(this);
                case PlayerRankEnum.C:
                    return new PlayerRankB(this);
                case PlayerRankEnum.B:
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
            _currentRank.Enter();
        }
    }


    
}