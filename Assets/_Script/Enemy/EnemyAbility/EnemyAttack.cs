// Author : Peiyu Wang @ Daphatus
// 05 12 2024 12 51

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Damageable;
using _Script.Utilities;
using DamageNumbersPro;
using UnityEngine;

namespace _Script.Enemy.EnemyAbility
{
    public abstract class EnemyAttack : MonoBehaviour, IEnemyAbilityHandler
    {
         //Spawn a circular raycast that will damage the player if it hits
        
        [SerializeField] protected float attackRange = 0.5f;
        public float AttackRange
        {
            get => attackRange;
            set
            {
                attackRange = value;
            }
        }
        [SerializeField] protected float damage = 10f; public float Damage
        {
            get => damage;
            set
            {
                damage = value;
            }
        }
        [SerializeField] private float damageCooldown = 1.5f;
        public float DamageCooldown //not sure if this is active variable
        {
            get => damageCooldown;
            set
            {
                damageCooldown = value;
            }
        }
        //Damageable tags
        private readonly List<string> _targetTags = new List<string>() {"Player"};
        //Damage Number
        [SerializeField] protected GameObject visualEffectPrefab;
        [SerializeField] protected DamageNumber damageNumberPrefab;
        public abstract void UseAbility(Transform target);
        
        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
 

        
        protected bool CanDamageTag(Collider2D other)
        {
            return _targetTags.Contains(other.tag);
        }
        

    }
}