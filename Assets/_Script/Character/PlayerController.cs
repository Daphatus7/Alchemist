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
        private PlayerCharacter _playerCharacter;
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
            _playerCharacter = GetComponent<PlayerCharacter>();
        }

        private void OnEnable()
        {
            _playerInputActions.Player.Enable();
            _playerInputActions.Player.Fire.performed += OnFire;
            _playerInputActions.Player.NormalAttack.performed += OnNormalAttack;
            _playerInputActions.Player.Move.performed += OnMove;
            _playerInputActions.Player.Move.canceled += OnMove;
        }

        private void OnDisable()
        {
            _playerInputActions.Player.Fire.performed -= OnFire;
            _playerInputActions.Player.NormalAttack.performed -= OnNormalAttack;
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
            _rigidbody2D.velocity = _movement * speed;
        }


        private void OnFire(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.RightMouseButton(_fireDirection);
            }
        }
        
        private void OnNormalAttack(InputAction.CallbackContext context)
        {
            foreach (var control in _controls)
            {
                control.LeftMouseButton(_fireDirection);
            }
        }

        private void UpdateFireDirection()
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            if (Camera.main != null)
            {
                Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                _fireDirection = worldPosition - transform.position;
            }
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _fireDirection);
        }
    }

}