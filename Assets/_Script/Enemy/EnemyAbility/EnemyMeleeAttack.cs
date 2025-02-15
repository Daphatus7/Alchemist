// Author : Peiyu Wang @ Daphatus
// 15 02 2025 02 55

using System.Collections;
using _Script.Damageable;
using _Script.Utilities;
using UnityEngine;

namespace _Script.Enemy.EnemyAbility
{
    public class EnemyMeleeAttack : EnemyAttack
    {
        public override void UseAbility(Transform target)
        {
            var targetPosition = target.position;
            Helper.CreateWorldText("Attacking", null, transform.position, 30, Color.white, TextAnchor.MiddleCenter);
            StartCoroutine(DelayedDamage(targetPosition));
        }
        private IEnumerator DelayedDamage(Vector2 targetPosition)
        {
            yield return new WaitForSeconds(0.3f);
            PlayVisualEffect(attackRange, targetPosition);
            
            //create logical attack
            Collider2D[] colliders = Physics2D.OverlapCircleAll(targetPosition, attackRange);

            //Apply damage to the first damageable object
            foreach (Collider2D other in colliders)
            {
                var damageable = other.GetComponent<IDamageable>();
                if (damageable != null && CanDamageTag(other))
                {
                    var actualDamage = Mathf.Abs(damageable.ApplyDamage(damage));
                    if (damageNumberPrefab)
                    {
                        damageNumberPrefab.Spawn(other.transform.position, actualDamage);
                    }
                    break;
                }
            }
        }
    
        private void PlayVisualEffect(float radius, Vector2 targetPosition, float duration = 0.5f)
        {
            if (visualEffectPrefab)
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
    }
    
    
}