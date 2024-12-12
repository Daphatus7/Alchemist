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
            _controls.AddRange(GetComponents<IControl>());
        }   
        
        protected virtual void AwakenInitialize()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            if (_rigidbody2D == null)
            {
                Debug.LogWarning("PlayerController: Missing Rigidbody2D component.");
            }

            _playerInputActions = new PlayerInputActions();
        }

        private void OnEnable()
        {
            var playerActions = _playerInputActions.Player;
            playerActions.Enable();

            // Mouse Buttons
            playerActions.RightMouseButton.performed += OnRightMouseButtonDown;
            playerActions.RightMouseButton.canceled += OnRightMouseButtonUp;
            
            playerActions.LeftMouseButton.performed += OnLeftMouseButtonDown;
            playerActions.LeftMouseButton.canceled += OnLeftMouseButtonUp;
            
            // Dash
            playerActions.Dash.performed += OnDash;
            
            // Sprint
            playerActions.Sprint.performed += OnSprint;
            playerActions.Sprint.canceled += OnSprintEnd;

            // Movement
            playerActions.Move.performed += OnMove;
            playerActions.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            var playerActions = _playerInputActions.Player;

            // Mouse Buttons
            playerActions.RightMouseButton.performed -= OnRightMouseButtonDown;
            playerActions.RightMouseButton.canceled -= OnRightMouseButtonUp;

            playerActions.LeftMouseButton.performed -= OnLeftMouseButtonDown;
            playerActions.LeftMouseButton.canceled -= OnLeftMouseButtonUp;
            
            // Dash
            playerActions.Dash.performed -= OnDash;

            // Sprint
            playerActions.Sprint.performed -= OnSprint;
            playerActions.Sprint.canceled -= OnSprintEnd;

            // Movement
            playerActions.Move.performed -= OnMove;
            playerActions.Move.canceled -= OnMove;

            playerActions.Disable();
        }

        private void Update()
        {
            UpdateFireDirection();
        }

        #region Input Callbacks

        private void OnMove(InputAction.CallbackContext context)
        {
            _movement = context.ReadValue<Vector2>();
            foreach (var m in _controls)
            {
                m.Move(_movement);
            }
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
                control.RightMouseButtonUp(_fireDirection);
            }
        }

        private void OnDash(InputAction.CallbackContext obj)
        {
            // Perform dash with current movement direction
            foreach (var c in _controls)
            {
                c.Dash(_movement);
            }
        }
        
        private void OnSprint(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.Sprint(_movement);
            }
        }

        private void OnSprintEnd(InputAction.CallbackContext obj)
        {
            foreach (var c in _controls)
            {
                c.SprintEnd(_movement);
            }
        }

        #endregion
        

        private void UpdateFireDirection()
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Camera mainCamera = Camera.main;
            if (mainCamera == null) return;
            
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
            _fireDirection = (worldPosition - transform.position);
            _fireDirection.Normalize();
        }

        

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _fireDirection);
        }
    }
}