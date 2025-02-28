// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 05

using System;
using System.Collections;
using _Script.Character.PlayerStat;
using _Script.Managers;
using UnityEngine;

namespace _Script.Character.PlayerStat
{
    /// <summary>
    /// Concrete implementation for Sanity.
    /// </summary>
    [Serializable]
    public class PlayerSanity : PlayerStat
    {
        [SerializeField] private float sanityRate = 0.5f;
        private void OnUpdateNight()
        {
            Modify(-sanityRate);
        }

        public override StatType StatType => StatType.Sanity;
        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }
        
        private void OnNewDay()
        {
        }
        
        private void OnNightStart()
        {
        }

        
        public override void OnEnabled()
        {
            base.OnEnabled();
            TimeManager.Instance.OnNewDay += OnNewDay;
            TimeManager.Instance.OnNightStart += OnNightStart;
            TimeManager.Instance.OnUpdateNight += OnUpdateNight;
        }

        public override void CleanUp()
        {
            base.CleanUp();
            TimeManager.Instance.OnNewDay -= OnNewDay;
            TimeManager.Instance.OnNightStart -= OnNightStart;
            TimeManager.Instance.OnUpdateNight -= OnUpdateNight;
        }

        public override void Reset()
        {
            CurrentValue = MaxValue;
        }
    }
}