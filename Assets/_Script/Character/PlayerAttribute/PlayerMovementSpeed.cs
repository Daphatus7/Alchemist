// Author : Peiyu Wang @ Daphatus
// 04 02 2025 02 09

using System;
using UnityEngine;

namespace _Script.Character.PlayerAttribute
{
    
    [Serializable]
    public class PlayerMovementSpeed : PlayerAttribute
    {
        public override AttributeType AttributeType => AttributeType.MovementSpeed;
        [SerializeField] private float sprintMultiplier = 1.5f;
        public float SprintMultiplier => sprintMultiplier;
        [SerializeField] private float dashSpeed = 10f;
        public float DashSpeed => dashSpeed;
        [SerializeField] private float dashDuration = 0.2f;
        public float DashDuration => dashDuration;
        [SerializeField] private float dashCooldown = 2f;
        public float DashCooldown => dashCooldown;
    }
}