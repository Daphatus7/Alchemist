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
        public PlayerSanity() : base()
        {
            TimeManager.Instance.onNewDay.AddListener(OnNewDay);
            TimeManager.Instance.onNightStart.AddListener(OnNightStart);
            TimeManager.Instance.onUpdateNight += OnUpdateNight;
        }

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
    }
}