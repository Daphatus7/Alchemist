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
        private PlayerCharacter _playerCharacter;
        private Vector2 _fireDirection;

        private void Awake()
        {
            AwakenInitialize();
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
            _playerCharacter?.Shoot(_fireDirection);
        }
        
        private void OnNormalAttack(InputAction.CallbackContext context)
        {
            _playerCharacter?.NormalAttack(_fireDirection);
        }

        private void UpdateFireDirection()
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
            _fireDirection = worldPosition - transform.position;
        }
        
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, _fireDirection);
        }
    }

}