using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Character.PlayerAttribute
{
    
    public enum StatType
    {
        Health,
        Mana,
        Stamina,
        Sanity,
        Food
    }
    
    /// <summary>
    /// Abstract base class for a player stat (such as health or mana).
    /// It holds the maximum and current values and provides an event
    /// for when the stat value changes.
    /// </summary>
    [Serializable]
    public abstract class PlayerStat
    {
        /// <summary>
        /// The maximum allowed value for the stat.
        /// </summary>
        public float MaxValue { get; protected set; }

        private float currentValue;
        protected float Threshold = 0.2f;
        
        private bool isBelowThreshold;

        /// <summary>
        /// Event fired when the stat value falls to or below the threshold.
        /// </summary>
        public event Action onBelowThreshold;
        
        /// <summary>
        /// Event fired when the stat value exceeds the threshold.
        /// </summary>
        public event Action onExceedingThreshold;
        
        public abstract StatType StatType { get; }
        /// <summary>
        /// The current value for the stat. It is clamped between 0 and MaxValue.
        /// When set, this property checks for crossing the threshold boundary
        /// and fires the appropriate events.
        /// </summary>
        public float CurrentValue
        {
            get => currentValue;
            protected set
            {
                // Save the old threshold state.
                bool oldBelow = isBelowThreshold;
                
                // Clamp and update currentValue.
                currentValue = Mathf.Clamp(value, 0, MaxValue);
                // Notify any listeners of the new value.
                OnValueChanged?.Invoke(currentValue);
                
                // Determine new threshold state.
                // (Here we consider “at threshold” as having reached the low state.)
                isBelowThreshold = currentValue <= Threshold;
                
                // Only fire an event if the state changed.
                if(oldBelow != isBelowThreshold)
                {
                    if(isBelowThreshold)
                    {
                        OnOnBelowThreshold();
                    }
                    else
                    {
                        OnOnExceedingThreshold();
                    }
                }
            }
        }
        
        

        /// <summary>
        /// Event fired when the stat's current value changes.
        /// </summary>
        public UnityAction<float> OnValueChanged;

        /// <summary>
        /// Constructor for the stat.
        /// </summary>
        /// <param name="maxValue">The maximum value for the stat.</param>
        public PlayerStat(float maxValue)
        {
            MaxValue = maxValue;
            CurrentValue = maxValue; 
            isBelowThreshold = false;
        }

        /// <summary>
        /// Modify the stat by a given amount. A negative amount indicates a decrease (e.g. damage),
        /// and a positive amount indicates a gain (e.g. healing).
        /// Returns the modified amount.
        /// </summary>
        /// <param name="amount">The amount to modify.</param>
        public abstract float Modify(float amount);

        protected virtual void OnOnBelowThreshold()
        {
            onBelowThreshold?.Invoke();
        }

        protected virtual void OnOnExceedingThreshold()
        {
            onExceedingThreshold?.Invoke();
        }
    }

    
}