using System;
using System.Collections.Generic;
using _Script.Character.Ability;
using _Script.Damageable;
using _Script.Items.AbstractItemTypes;
using _Script.Movement;
using UnityEngine;

namespace _Script.Weapon
{
    /**
     * Weapon is a type of attack that can be equipped by a character
     */
    [RequireComponent(typeof(BoxCollider2D))]
    public class Weapon : Damage
    {
        [SerializeField] private float attackCooldown; public float AttackCooldown => attackCooldown;
        protected bool isCoolingDown = false; public bool IsCoolingDown => isCoolingDown;
        
        public void SetWeaponItem(WeaponItem weaponItem, List<string> targetTags)
        {
            SetDamage(weaponItem.damage);
            attackCooldown = weaponItem.attackSpeed;
            SetTargetType(targetTags);
        }
        
        protected virtual void Attack(Vector2 direction)
        {
            
        }
        
        public void OnPressed(Vector2 direction)
        {
            Attack(direction);
        }
        
        public void OnReleased(Vector2 direction)
        {
        }
        
    }
}