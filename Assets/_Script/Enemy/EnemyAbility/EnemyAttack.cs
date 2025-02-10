// Author : Peiyu Wang @ Daphatus
// 05 12 2024 12 51

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Damageable;
using DamageNumbersPro;
using UnityEngine;

namespace _Script.Enemy.EnemyAbility
{
    public class EnemyAttack : MonoBehaviour, IEnemyAbilityHandler
    {
         //Spawn a circular raycast that will damage the player if it hits
        
        [SerializeField] private float attackRange = 0.5f;
        [SerializeField] private float damage = 10f;
        [SerializeField] private float damageCooldown = 1.5f;
        //Damageable tags
        private readonly List<string> _targetTags = new List<string>() {"Player"};
        //Damage Number
        [SerializeField] private GameObject visualEffectPrefab;
        
        [SerializeField] private DamageNumber damageNumberPrefab;

        
        public void UseAbility(Transform target)
        {
            var targetPosition = target.position;
            StartCoroutine(DelayedDamage(targetPosition));
        }
        
        private IEnumerator DelayedDamage(Vector2 targetPosition)
        {
            yield return new WaitForSeconds(0.3f);
            PlayVisualEffect(attackRange, targetPosition);
            Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPosition, attackRange);
            
            //Gizmo debug circle

            foreach (Collider2D other in colliders)
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null && CanDamageTag(other))
                {
                    var actualDamage = Mathf.Abs(damageable.ApplyDamage(damage));
                    if (damageNumberPrefab != null)
                    {
                        damageNumberPrefab.Spawn(other.transform.position, actualDamage);
                    }
                    break;
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }


        private void PlayVisualEffect(float radius, Vector2 targetPosition, float duration = 0.5f)
        {
            if (visualEffectPrefab != null)
            {
                GameObject visualEffect = Instantiate(visualEffectPrefab, targetPosition, Quaternion.identity);
                visualEffect.transform.localScale = new Vector3(radius, radius, 1);
                StartCoroutine(VisualFadeOut(visualEffect.GetComponentInChildren<SpriteRenderer>(), duration));
            }
        }
        
        private IEnumerator VisualFadeOut(SpriteRenderer visualEffect, float duration)
        {
            float elapsedTime = 0;
            Color color = visualEffect.color;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(1, 0, elapsedTime / duration);
                visualEffect.color = color;
                yield return null;
            }
            Destroy(visualEffect.gameObject);
        }
        
        protected bool CanDamageTag(Collider2D other)
        {
            return _targetTags.Contains(other.tag);
        }
    }
}