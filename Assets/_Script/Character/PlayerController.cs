using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    public class PlayerController : MonoBehaviour
    {

        [SerializeField] private float speed = 5f;
        private Rigidbody2D _rigidbody2D;
        private Vector2 _movement;
        private PlayerInputActions _playerInputActions;
        private Vector2 _fireDirection;
        
        private List<IControl> _controls = new List<IControl>();

        private void Awake()
        {
            AwakenInitialize();
            //get all controls in the children
            _controls.AddRange(GetComponents<IControl>());

        }   
        
        protected virtual void AwakenInitialize()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _playerInputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
            _playerInputActions.Player.RightMouseButton.performed += OnRightMouseButtonDown;
            _playerInputActions.Player.RightMouseButton.canceled += OnRightMouseButtonUp;
            _playerInputActions.Player.LeftMouseButton.performed += OnLeftMouseButtonDown;
            _playerInputActions.Player.LeftMouseButton.canceled += OnLeftMouseButtonUp;

            _playerInputActions.Player.Move.performed += OnMove;
            _playerInputActions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            _playerInputActions.Player.RightMouseButton.performed -= OnRightMouseButtonDown;
            _playerInputActions.Player.LeftMouseButton.performed -= OnLeftMouseButtonDown;
            _playerInputActions.Player.RightMouseButton.canceled -= OnRightMouseButtonUp;
            _playerInputActions.Player.LeftMouseButton.canceled -= OnLeftMouseButtonUp;
            _playerInputActions.Player.Disable();

            _playerInputActions.Player.Move.performed -= OnMove;
            _playerInputActions.Player.Move.canceled -= OnMove;
        }

        private void Update()
        {
            UpdateMovement();
            UpdateFireDirection();
            
        }
        
        
        private void OnMove(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
        }

        private void UpdateMovement()
        {
            _rigidbody2D.linearVelocity = _movement * speed;
        }


        private void OnLeftMouseButtonDown(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.LeftMouseButtonDown(_fireDirection);
            }
        }
        
        private void OnLeftMouseButtonUp(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.LeftMouseButtonUp(_fireDirection);
            }
        }
        
        private void OnRightMouseButtonDown(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.RightMouseButtonDown(_fireDirection);
            }
        }
        
        private void OnRightMouseButtonUp(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.LeftMouseButtonUp(_fireDirection);
            }
        }

        private void UpdateFireDirection()
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            if (Camera.main != null)
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                _fireDirection = (worldPosition - transform.position).normalized;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _fireDirection);
        }
    }

}