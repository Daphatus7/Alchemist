// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 39

using System;
using System.Collections.Generic;
using _Script.Character.PlayerStat;
using _Script.Character.PlayerStateMachine;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Character
{
    public interface IPlayerStatsManagerHandler
    {
        PlayerStat.PlayerStat GetStat(StatType statType);
        /// <summary>
        /// unsubscribe all events when needed 
        /// </summary>
        void UnsubscribeAll();
    }
    
    /// <summary>
    /// PlayerStatsManager encapsulates all the core stats for the player, such as health, mana, food, sanity, and stamina.
    /// It provides a central point to access and modify player data.
    /// </summary>
    [Serializable]
    public class PlayerStatsManager: IPlayerStatsManagerHandler
    {
        
        private Dictionary<StatType, PlayerStat.PlayerStat> _playerStats; public Dictionary<StatType, PlayerStat.PlayerStat> PlayerStats => _playerStats;
        
        [SerializeField, LabelText("Health Stat")]
        private PlayerHealth health;
        
        [SerializeField, LabelText("Mana Stat")]
        private PlayerMana mana;
        
        [SerializeField, LabelText("Food Stat")]
        private FoodStat food;
        
        [SerializeField, LabelText("Sanity Stat")]
        private PlayerSanity sanity;
        
        [SerializeField, LabelText("Stamina Stat")]
        private PlayerStamina stamina;
        
        
        private List<PlayerState> _playerStates = new List<PlayerState>();
        /// <summary>
        /// Event invoked whenever any stat is modified.
        /// Subscribers (like UI) can update their displays.
        /// </summary>
        public event Action<StatType> OnStatsChanged; 
        public event Action OnDeath;
        
        public void UpdateState()
        {
            foreach (var state in _playerStates)
            {
                state.UpdateState();
            }
        }

        public void Initialize()
        {
            
            _playerStats = new Dictionary<StatType, PlayerStat.PlayerStat>
            {
                {StatType.Health, health},
                {StatType.Mana, mana},
                {StatType.Food, food},
                {StatType.Sanity, sanity},
                {StatType.Stamina, stamina}
            };
            
            _playerStates.Add(new PlayerFoodState(this));
            _playerStates.Add(new PlayerSanityState(this));
            _playerStates.Add(new PlayerStaminaState(this));

            // Subscribe to each stat's change event to relay a unified OnStatsChanged event.
            health.OnValueChanged += (val) => InvokeOnStatsChanged(health.StatType);
            health.OnDeath += InvokeOnDeath;
            mana.OnValueChanged += (val) => InvokeOnStatsChanged(mana.StatType);
            food.OnValueChanged += (val) => InvokeOnStatsChanged(food.StatType);
            sanity.OnValueChanged += (val) => InvokeOnStatsChanged(sanity.StatType);
            stamina.OnValueChanged += (val) => InvokeOnStatsChanged(stamina.StatType);
        }
        

 
        public float CurrentStamina => stamina.CurrentValue;


        /// <summary>
        /// Apply damage to the player.
        /// </summary>
        /// <param name="damage">Damage amount (negative modification)</param>
        public float TakeDamage(float damage)
        {
            return health.Modify(-damage);
        }

        /// <summary>
        /// Heal the player.
        /// </summary>
        /// <param name="amount">Healing amount (positive modification)</param>
        public void AddHealth(float amount)
        {
            health.Modify(amount);
        }

        /// <summary>
        /// Consume mana (returns true if successful).
        /// </summary>
        public bool ConsumeMana(float cost)
        {
            if (mana.CanConsume(cost))
            {
                mana.Modify(-cost);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore mana.
        /// </summary>
        public void RestoreMana(float amount)
        {
            mana.Modify(amount);
        }

        /// <summary>
        /// Consume food (returns true if successful).
        /// </summary>
        public bool ConsumeFood(float amount)
        {
            if (food.CurrentValue >= amount)
            {
                food.Modify(-amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore food.
        /// </summary>
        public void RestoreFood(float amount)
        {
            food.Modify(amount);
        }

        /// <summary>
        /// Consume stamina (returns true if successful).
        /// </summary>
        public bool ConsumeStamina(float amount)
        {
            if (stamina.CurrentValue >= amount)
            {
                stamina.Modify(-amount);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Restore stamina.
        /// </summary>
        public void RestoreStamina(float amount)
        {
            stamina.Modify(amount);
        }

        /// <summary>
        /// Modify sanity (can be used for buffs or debuffs).
        /// </summary>
        public void AddSanity(float amount)
        {
            sanity.Modify(amount);
        }
        
        #region Invoke Events
        
        public void InvokeOnStatsChanged(StatType statType)
        {
            OnStatsChanged?.Invoke(statType);
        }
        
        public void InvokeOnDeath()
        {
            OnDeath?.Invoke();
        }

        #endregion

        #region Player Stats Interface
        
        private List<PlayerStateFlagType> _playerStateFlagTypes = new List<PlayerStateFlagType>();

        public PlayerStat.PlayerStat GetStat(StatType statType)
        {
            return _playerStats[statType];
        }

        /// <summary>
        /// Unsubscribe all events when needed
        /// </summary>
        public void UnsubscribeAll()
        {
            foreach (var state in _playerStates)
            {
                state.CleanUp();
            }

            foreach (var stat in _playerStats)
            {
                stat.Value.CleanUp();
            }
        }

        #endregion

    }
}