// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 40

using System;
using _Script.Character.PlayerStat;

namespace _Script.Character.PlayerStat
{
    /// <summary>
    /// Concrete implementation for Food.
    /// </summary>
    [Serializable]
    public class FoodStat : PlayerStat
    {
        public override StatType StatType => StatType.Food;
        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }
    }
}