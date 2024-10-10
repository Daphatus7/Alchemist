using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        
        [SerializeField] private float speed = 20f;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _movement;
        private PlayerInputActions _playerInputActions;
        
        
        private void Awake()
        {
            AwakenInitialize();
        }
        
        protected virtual void AwakenInitialize()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _playerInputActions = new PlayerInputActions();
        }
        
        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
            _playerInputActions.Player.Fire.performed += OnFire;
            
            _playerInputActions.Player.Move.performed += OnMove;
            _playerInputActions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Fire.performed -= OnFire;
            _playerInputActions.Player.Disable();
            
  
            _playerInputActions.Player.Move.performed -= OnMove;
            _playerInputActions.Player.Move.canceled -= OnMove;
        }
        
        private void Update()
        {
            UpdateMovement();
        }
        
        private void OnMove(InputAction.CallbackContext context)
        {
            Debug.Log("Move action performed");
            _movement = context.ReadValue<Vector2>();
        }
        
        
        private void UpdateMovement()
        {
            _rigidbody2D.MovePosition(_rigidbody2D.position + _movement * (speed * Time.fixedDeltaTime));
        }


        private void OnFire(InputAction.CallbackContext context)
        {
            Debug.Log("Fire action performed");
            // Add your fire logic here
        }
        
    }
}