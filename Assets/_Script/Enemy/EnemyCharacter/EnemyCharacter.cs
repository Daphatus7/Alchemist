using System;
using _Script.Attribute;
using _Script.Character.PlayerRank;
using _Script.Damageable;
using _Script.Drop;
using _Script.Enemy.EnemyAbility;
using Pathfinding;
using Unity.Behavior;
using UnityEngine;

namespace _Script.Enemy.EnemyCharacter
{
    public sealed class EnemyCharacter : MonoBehaviour, IDamageable
    {
        
        [SerializeField] private EnemyData.EnemyData enemyData;
        [SerializeField] private AIPath _agent;
        [SerializeField] private EnemyAttack _attack;
        private float _health; private float Health
        {
            get => _health;
            set
            {
                _health = value;
                if (_health <= 0)
                {
                    OnDeath();
                }
            }
        }
        
        private float _damage; public float Damage
        {
            get => _damage;
            set
            {
                _damage = value;
                if(_attack)
                {
                    _attack.Damage = _damage;
                }
            }
        }
        private float _moveSpeed; public float MoveSpeed
        {
            get => _moveSpeed;
            set
            {
                _moveSpeed = value;
                if (_agent)
                    _agent.maxSpeed = _moveSpeed;
            }
        }
        
        private float _attackRange; public float AttackRange
        {
            get => _attackRange;
            set
            {
                _attackRange = value;
                if (_attack)
                {
                    _attack.AttackRange = _attackRange;
                }
            }
        }
        
        private float _damageCooldown; public float DamageCooldown
        {
            get => _damageCooldown;
            set
            {
                _damageCooldown = value;
                if (_attack)
                {
                    _attack.DamageCooldown = _damageCooldown;
                }
            }
        }
        
        public float ApplyDamage(float damage)
        {
            return Health -= damage;
        }
        
        public void Initialize(PlayerRankEnum rank)
        {
            var attribute = enemyData.CreateScaledAttribute(rank);
            Health = attribute.Health;
            Damage = attribute.Damage;
            MoveSpeed = attribute.MoveSpeed;
            AttackRange = attribute.AttackRange;
            DamageCooldown = attribute.AttackFrequency;
        }

        private void OnDeath()
        {
            GetComponent<DropItemComponent>()?.DropItems();
            Destroy(gameObject);
        }
    }

}