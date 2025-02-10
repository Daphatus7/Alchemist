using _Script.Character.PlayerStat;
using _Script.Damageable;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Attribute
{
    public class PawnAttribute : MonoBehaviour, IDamageable
    {
        [SerializeField] protected PlayerHealth health; public PlayerHealth Health => health;
        [SerializeField] protected float healthMax = 100f; public float HealthMax => healthMax;
        
        //event on health change
        protected UnityEvent onHealthChanged = new UnityEvent();
        
        public virtual float ApplyDamage(float damage)
        {
            return health.Modify(-damage);
        }

        protected virtual void OnDeath()
        {
            Debug.LogError("Die method has not been implemented yet.");
        }
        
        protected void RestoreHealth(float value)
        {
            health.Modify(-value);     
        }
        
    }

    public enum AttributeType
    {
        Health,
        Mana,
        Stamina,
        Sanity,
        Food
    }
}