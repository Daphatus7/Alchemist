using System.Collections.Generic;
using System.Linq;
using DamageNumbersPro;
using UnityEngine;

namespace _Script.Damageable
{
    /**
     * This class is responsible for dealing damage to other objects.
     */
    [RequireComponent(typeof(Collider2D))]
    public class Damage: MonoBehaviour
    {
        [Header("Damage")] [SerializeField] private float damage = 10f;
        public float DamageValue => damage;
        protected void SetDamage(float value)
        {
            damage = value;
        }
        [SerializeField] private DamageNumber numberPrefab;
        private List<string> _targetTags = new List<string>();
        
        public void SetTargetType(List<string> targetTags)
        {
            _targetTags = targetTags;
        }

        protected virtual bool CanDamage()
        {
            return true;
        }
        
        protected virtual void OnTriggerStay2D(Collider2D other)
        {
            TryDamage(other);
        }
        
        protected virtual void TryDamage(Collider2D other)
        {
            if(!CanDamage()) return;
            if (!IsTarget(other) || !other.TryGetComponent(out IDamageable d)) return;
            Debug.Log("Dealing damage to " + other.name);
            var actualDamage = d.ApplyDamage(damage);
            PlayDamageEffect(actualDamage, other);
        }
        
        private bool IsTarget(Collider2D other)
        {
            return _targetTags.Contains(other.tag);
        }
        
        protected virtual void PlayDamageEffect(float actualDamage, Collider2D other)
        {
            numberPrefab.Spawn(other.transform.position, actualDamage);
        }
    }
}