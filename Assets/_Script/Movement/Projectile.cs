using System.Collections;
using UnityEngine;

namespace _Script.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifeTime = 2f;
        private Rigidbody2D _rigidbody2D;
        private void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
        
        private void OnEnable()
        {
            StartCoroutine(UpdateMovementCoroutine());
        }

        private IEnumerator UpdateMovementCoroutine()
        {
            UpdateMovement();
            yield return new WaitForSeconds(lifeTime);
            Destroy(gameObject);
        }
        
        private void UpdateMovement()
        {
            _rigidbody2D.velocity = transform.right * speed;
        }

        
    }
}