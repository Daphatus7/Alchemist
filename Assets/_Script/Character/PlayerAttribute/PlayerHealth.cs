// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 29

using System;
using UnityEngine.Events;

namespace _Script.Character.PlayerAttribute
{
    /// <summary>
    /// Concrete implementation for Health.
    /// </summary>
    [Serializable]
    public class HealthStat : PlayerStat
    {
        /// <summary>
        /// Event that can be used to trigger player death when health reaches zero.
        /// </summary>
        public UnityAction OnDeath;
        
        public override StatType StatType => StatType.Health;

        public override float Modify(float amount)
        {
            CurrentValue += amount;
            if (CurrentValue <= 0)
            {
                CurrentValue = 0;
                OnDeath?.Invoke();
            }
            return amount;
        }
        
        /// <summary>
        /// If health reaches zero, trigger death event.
        /// </summary>
        /// <param name="amount"></param>
        public override void DecreaseMaxValue(float amount)
        {
            base.DecreaseMaxValue(amount);
            if(MaxValue <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public bool IsDead => CurrentValue <= 0;
    }
}