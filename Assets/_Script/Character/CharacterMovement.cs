using UnityEngine;

namespace _Script.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterMovement : MonoBehaviour
    {
        private float _speed = 5f;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _movement;
        
        private void Awake()
        {
            AwakenInitialize();
        }
        
        protected virtual void AwakenInitialize()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
        }
        
        private void Update()
        {
            UpdateMovementInput();
            
            UpdateMovement();
        }
        
        protected virtual void Move()
        {
            _rigidbody2D.MovePosition(_rigidbody2D.position + _movement * (_speed * Time.fixedDeltaTime));
        }
        
        private void UpdateMovementInput()
        {
            var horizontal = Input.GetAxis("Horizontal");
            var vertical = Input.GetAxis("Vertical");
            _movement = new Vector2(horizontal, vertical);
        }
        
        private void UpdateMovement()
        {
            Move();
        }
        
        
    }
}