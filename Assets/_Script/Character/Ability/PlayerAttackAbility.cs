using System.Collections.Generic;
using UnityEngine;

namespace _Script.Character.Ability
{
    [RequireComponent(typeof(Attribute.PlayerAttribute))]
    public class PlayerAttackAbility : AttackAbility
    {
        
        private float _attackCooldown;
        private float _attackTimer;
        
        private void Awake()
        {
            targetTags = new List<string> {"Enemy"};
        }
    }
}