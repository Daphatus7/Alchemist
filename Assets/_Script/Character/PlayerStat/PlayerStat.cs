using System;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Character.PlayerStat
{
    public enum StatType
    {
        Health,
        Mana,
        Stamina,
        Sanity,
        Food
    }
    [Serializable]
    public abstract class PlayerStat
    {
        [SerializeField]
        private float maxValue = 100f;
        
        public float MaxValue 
        { 
            get => maxValue; 
            protected set => maxValue = value; 
        }

        [SerializeField]
        private float threshold = 0.2f;
        public float Threshold 
        { 
            get => threshold; 
            protected set => threshold = value; 
        }
        
        [SerializeField]
        private float currentValue;
        public float CurrentValue
        {
            get => currentValue;
            protected set
            {
                bool oldBelow = currentValue <= threshold;
                currentValue = Mathf.Clamp(value, 0, maxValue);
                OnValueChanged?.Invoke(currentValue);
                bool newBelow = currentValue <= threshold;
                if (oldBelow != newBelow)
                {
                    if(newBelow)
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
        
        public UnityAction<float> OnValueChanged;
        public event Action onBelowThreshold;
        public event Action onAboveThreshold;
        public PlayerStat( )
        {
            currentValue = maxValue; 
        }
        
        public PlayerStat(float maxValue, float currentValue)
        {
            this.maxValue = maxValue;
            this.currentValue = currentValue;
        }

        public abstract StatType StatType { get; }
        
        public abstract float Modify(float amount);

        protected virtual void OnOnBelowThreshold()
        {
            onBelowThreshold?.Invoke();
        }

        protected virtual void OnOnExceedingThreshold()
        {
            onAboveThreshold?.Invoke();
        }
        
        public void IncreaseMaxValue(float amount)
        {
            MaxValue += amount;
        }
        public virtual void DecreaseMaxValue(float amount)
        {
            MaxValue -= amount;
            if (CurrentValue > MaxValue)
            {
                CurrentValue = MaxValue;
            }
            if (MaxValue <= 0)
            {
                MaxValue = 0;
                CurrentValue = 0;
            }
        }
        /// <summary>
        /// To initialize components after other systems have been initialized.
        /// </summary>
        public virtual void OnEnabled()
        {
            
        }
        
        /// <summary>
        /// To clean up components before the system is destroyed.
        /// </summary>
        public virtual void CleanUp()
        {
            OnValueChanged = null;
            onBelowThreshold = null;
            onAboveThreshold = null;
        }
    }
}