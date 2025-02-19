
using _Script.Character.PlayerRank;
using _Script.Damageable;
using _Script.Drop;
using _Script.Enemy.EnemyAbility;
using _Script.Quest;
using Pathfinding;
using Unity.Behavior;
using UnityEngine;

namespace _Script.Enemy.EnemyCharacter
{
    public sealed class EnemyCharacter : MonoBehaviour, IDamageable
    {
        
        [SerializeField] private EnemyData.EnemyData enemyData;
        [SerializeField] private EnemyAttack _attack;
        [SerializeField] private AIPath _agent;


        [SerializeField] private string enemyID;

        private float _health = 50; private float Health
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
        
        [SerializeField] private float _moveSpeed = 3;

        public float MoveSpeed
        {
            get
            {
                Debug.Log("Getting move speed" + _moveSpeed);
                return _moveSpeed;
            }
            private set
            {
                if(_agent)
                {
                    _agent.maxSpeed = value;
                }
                _moveSpeed = value;
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

            Health -= damage;
            return damage;
        }


        [SerializeField] private bool _debug;
        
        public void Start()
        {
            if (_debug)
            {
                Initialize(NiRank.S);
            }
        }
        
        
        public void Initialize(NiRank rank)
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
            QuestManager.Instance.OnEnemyKilled(enemyID);
            GetComponent<DropItemComponent>()?.DropItems();
            Destroy(gameObject);
        }
    }

}