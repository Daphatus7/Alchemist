// Author : Peiyu Wang @ Daphatus
// 03 02 2025 02 40

using System;

namespace _Script.Character.PlayerAttribute
{
    /// <summary>
    /// Concrete implementation for Food.
    /// </summary>
    [Serializable]
    public class FoodStat : PlayerStat
    {
        public FoodStat(float maxFood) : base(maxFood) { }
        public override StatType StatType => StatType.Food;
        public override float Modify(float amount)
        {
            CurrentValue += amount;
            return amount;
        }
    }
}