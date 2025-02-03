// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 09

using System;

namespace _Script.Character.PlayerAttribute
{
    
    [Serializable]
    public class PlayerMovementSpeed : PlayerAttribute
    {
        public override AttributeType AttributeType => AttributeType.MovementSpeed;
    }
}