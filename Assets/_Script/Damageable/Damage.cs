using System;
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

        public void Awake()
        {
            //set collider to only include _targetTags
            var collider = GetComponent<Collider2D>();
            
        }

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
            Debug.Log("TryDamage");
            if(!CanDamage()) return;
            Debug.Log("CanDamage");
            if (!IsTarget(other) || !other.TryGetComponent(out IDamageable d)) return;
            Debug.Log("IsTarget");
            var actualDamage = d.ApplyDamage(damage);
            PlayDamageEffect(actualDamage, other);
        }
        
        private bool IsTarget(Collider2D other)
        {
            Debug.Log(other);
            Debug.Log(other.tag);
            Debug.Log(_targetTags);
            return _targetTags.Contains(other.tag);
        }
        
        protected virtual void PlayDamageEffect(float actualDamage, Collider2D other)
        {
            numberPrefab.Spawn(other.transform.position, actualDamage);
        }
    }
}