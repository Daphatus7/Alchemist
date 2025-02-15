using System.Collections;
using _Script.Damageable;
using UnityEngine;

namespace _Script.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : Damage
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifeTime = 2f;

        private Rigidbody2D _rigidbody2D;

        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }

        /// <summary>
        /// Fires the projectile in the given direction at the set speed.
        /// </summary>
        public void Fire(Vector2 direction)
        {
            transform.right = direction.normalized; 
            
            // Set the velocity directly
            _rigidbody2D.linearVelocity = direction.normalized * speed;

            // Destroy after lifetime
            StartCoroutine(DestroyAfterLifetime());
        }

        private IEnumerator DestroyAfterLifetime()
        {
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }
    }
}