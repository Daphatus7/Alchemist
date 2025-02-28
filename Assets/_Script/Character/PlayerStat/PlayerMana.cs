// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 33

using System;
using _Script.Character.PlayerStat;

namespace _Script.Character.PlayerStat
{
    /// <summary>
    /// Concrete implementation for Mana.
    /// </summary>
    [Serializable]
    public class PlayerMana : PlayerStat
    {
        public PlayerMana() : base() { }
        
        public override StatType StatType => StatType.Mana;
        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }

        public override void Reset()
        {
            CurrentValue = MaxValue;
        }

        public bool CanConsume(float cost) => CurrentValue >= cost;
    }
}