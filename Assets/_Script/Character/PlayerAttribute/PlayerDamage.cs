// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 20

using System;

namespace _Script.Character.PlayerAttribute
{
    [Serializable]
    public class PlayerDamage : PlayerAttribute
    {
        public override AttributeType AttributeType => AttributeType.Damage;
    }
}