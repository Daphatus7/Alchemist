using System;
using _Script.Damageable;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Attribute
{
    public class PawnAttribute : MonoBehaviour, IDamageable
    {
        [SerializeField] protected float health = 100f; public float Health => health;
        [SerializeField] protected float healthMax = 100f; public float HealthMax => healthMax;
        
        //event on health change
        protected UnityEvent onHealthChanged = new UnityEvent();
        
        public float ApplyDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
                onHealthChanged?.Invoke();
                return damage;
            }
            onHealthChanged?.Invoke();
            return damage;
        }

        private void Die()
        {
            Debug.LogError("Die method has not been implemented yet.");
        }

        public void Restore(AttributeType type, float value)
        {
            switch (type)
            {
                case AttributeType.Health:
                    RestoreHealth(value);
                    break;
                case AttributeType.Mana:
                    //todo: implement mana restore
                    break;
                case AttributeType.Stamina:
                    //todo: implement stamina restore
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        
        private void RestoreHealth(float value)
        {
            //consider negative value
            health += value;
            if (health > healthMax)
            {
                health = healthMax;
            }
            else if (health < 0)
            {
                health = 0;
            }
            onHealthChanged?.Invoke();        
        }
        
    }

    public enum AttributeType
    {
        Health,
        Mana,
        Stamina,
    }
}