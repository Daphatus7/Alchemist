using System.Collections.Generic;
using DamageNumbersPro;
using UnityEngine;

namespace _Script.Damageable
{
    [RequireComponent(typeof(Collider2D))]
    public class Damage: MonoBehaviour
    {
        [Header("Damage")]
        [SerializeField] private float damage = 10f;
        [SerializeField] private DamageNumber numberPrefab;
        
        protected void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log("other: " + other.name);
            if(other.TryGetComponent(out IDamageable d))
            {
                var actualDamage = d.TakeDamage(damage);
                numberPrefab.Spawn(transform.position, actualDamage);
            }
        }
    }
}