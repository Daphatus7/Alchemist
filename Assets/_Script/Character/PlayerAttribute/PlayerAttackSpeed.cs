// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 05

using System;

namespace _Script.Character.PlayerAttribute
{
    [Serializable]
    public class PlayerAttackSpeed : PlayerAttribute
    {
        public override AttributeType AttributeType => AttributeType.AttackSpeed;
    }
}