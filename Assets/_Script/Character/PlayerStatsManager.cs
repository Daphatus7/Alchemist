// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 39

using System;
using System.Collections.Generic;
using _Script.Character.PlayerAttribute;
using UnityEngine;

namespace _Script.Character
{
    /// <summary>
    /// PlayerStatsManager encapsulates all the core stats for the player, such as health, mana, food, sanity, and stamina.
    /// It provides a central point to access and modify player data.
    /// </summary>
    [Serializable]
    public class PlayerStatsManager  
    {
        
        private Dictionary<StatType, PlayerStat> _playerStats;
        public Dictionary<StatType, PlayerStat> PlayerStats => _playerStats;
        
        [SerializeField] private HealthStat health;
        [SerializeField] private PlayerMana mana;
        [SerializeField] private FoodStat food;
        [SerializeField] private PlayerSanity sanity;
        [SerializeField] private PlayerStamina stamina;

        /// <summary>
        /// Event invoked whenever any stat is modified.
        /// Subscribers (like UI) can update their displays.
        /// </summary>
        public event Action<StatType> OnStatsChanged; 
        public event Action OnDeath;
        
        public void UpdateState()
        {
            foreach (var stat in _playerStats)
            {
                stat.Value.UpdateState();
            }
        }

        /// <summary>
        /// Initializes the player's stats with provided maximum values.
        /// </summary>
        public PlayerStatsManager(float maxHealth, float maxMana, float maxFood, float maxSanity, float maxStamina)
        {
            health = new HealthStat(maxHealth);
            mana = new PlayerMana(maxMana);
            food = new FoodStat(maxFood);
            sanity = new PlayerSanity(maxSanity);
            stamina = new PlayerStamina(maxStamina);
            
            _playerStats = new Dictionary<StatType, PlayerStat>
            {
                {StatType.Health, health},
                {StatType.Mana, mana},
                {StatType.Food, food},
                {StatType.Sanity, sanity},
                {StatType.Stamina, stamina}
            };

            // Subscribe to each stat's change event to relay a unified OnStatsChanged event.
            health.OnValueChanged += (val) => InvokeOnStatsChanged(health.StatType);
            health.OnDeath += InvokeOnDeath;
            mana.OnValueChanged += (val) => InvokeOnStatsChanged(mana.StatType);
            
            
            food.OnValueChanged += (val) => InvokeOnStatsChanged(food.StatType);
            
            
            
            sanity.OnValueChanged += (val) => InvokeOnStatsChanged(sanity.StatType);
            stamina.OnValueChanged += (val) => InvokeOnStatsChanged(stamina.StatType);
        }

        // Expose properties for reading current values.
        public float CurrentHealth => health.CurrentValue;
        public float CurrentMana => mana.CurrentValue;
        public float CurrentFood => food.CurrentValue;
        public float CurrentSanity => sanity.CurrentValue;
        public float CurrentStamina => stamina.CurrentValue;

        public float MaxHealth => health.MaxValue;
        public float MaxMana => mana.MaxValue;
        public float MaxFood => food.MaxValue;
        public float MaxSanity => sanity.MaxValue;
        public float MaxStamina => stamina.MaxValue;

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
        public void Heal(float amount)
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
    }
}