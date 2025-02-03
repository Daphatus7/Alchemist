// Author : Peiyu Wang @ Daphatus
// 02 02 2025 02 33

using System;

namespace _Script.Character.PlayerAttribute
{
    /// <summary>
    /// Concrete implementation for Mana.
    /// </summary>
    [Serializable]
    public class PlayerMana : PlayerStat
    {
        public PlayerMana(float maxMana) : base(maxMana) { }
        
        public override StatType StatType => StatType.Mana;
        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }

        public bool CanConsume(float cost) => CurrentValue >= cost;
    }
}