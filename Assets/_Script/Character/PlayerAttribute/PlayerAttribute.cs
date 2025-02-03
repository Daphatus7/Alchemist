// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 09

using System;
using UnityEngine;

namespace _Script.Character.PlayerAttribute
{
    
    /// <summary>
    /// Attributes such as Movement speed, attack speed and Stamina.
    /// </summary>
    [Serializable]
    public abstract class PlayerAttribute
    {
        [SerializeField] private float maxValue = 30f;
        
        public float MaxValue 
        { 
            get => maxValue; 
            protected set => maxValue = value; 
        }
        
        [SerializeField] private float minValue = 0f;
        
        [SerializeField]
        private float currentValue; public float CurrentValue
        {
            get => currentValue;
            set
            {
                currentValue = Mathf.Clamp(value, minValue, maxValue);
                OnValueChanged?.Invoke(currentValue);
            }
        }
        public abstract AttributeType AttributeType { get; }
        
        /// <summary>
        /// Probably don't need this.
        /// </summary>
        public event Action<float> OnValueChanged;
    }

    public enum AttributeType
    {
        MovementSpeed,
        AttackSpeed,
        Damage,
    }
}