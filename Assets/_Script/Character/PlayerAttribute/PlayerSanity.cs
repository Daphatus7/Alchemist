// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 05

using System;
using System.Collections;
using _Script.Managers;
using UnityEngine;

namespace _Script.Character.PlayerAttribute
{
    /// <summary>
    /// Concrete implementation for Sanity.
    /// </summary>
    [Serializable]
    public class PlayerSanity : PlayerStat
    {
        

        private void OnUpdateNight()
        {
            Modify(-0.1f);
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
    }
}