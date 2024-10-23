using _Script.Attribute;
using _Script.Character.Ability;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryHandles;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    public class PlayerCharacter : PlayerAttribute, IControl, IPlayerHandler
    {
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;
        private PlayerAttack _attackAbility;
        private Vector3 _CursorPosition; public Vector3 CursorPosition => _CursorPosition;
        private float _mouseAngle; public float MouseAngle => _mouseAngle;
        private float _facingDirection; public float FacingDirection => _facingDirection;

        private IPlayerInventoryHandle _playerInventory;
        private IEquipmentInventoryHandle _playerEquipment;
        
        private void Awake()
        {
            _attackAbility = GetComponent<PlayerAttack>();
            _playerInventory = GetComponentInChildren<PlayerInventory>();
            _playerEquipment = GetComponent<PlayerEquipmentInventory>();
        }


        private void Update()
        {
            UpdateCursorPosition();
        }
        
        private void UpdateCursorPosition()
        {
            _CursorPosition = Camera.main!.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _mouseAngle = Mathf.Atan2(_CursorPosition.y - transform.position.y, _CursorPosition.x - transform.position.x) * Mathf.Rad2Deg;
        }
        
        
        /**
         * Called when the left mouse button is pressed and holding
         */
        public void LeftMouseButtonDown(Vector2 direction)
        {
            _attackAbility.Pressed(direction);
        }
        
        /**
         * Release the right mouse button
         */
        public void LeftMouseButtonUp(Vector2 direction)
        {
            _attackAbility.Released(direction);
        }


        /**
         * Release the left mouse button
         */
        public void RightMouseButtonUp(Vector2 direction)
        {
        }
        
        /**
         * Called when the right mouse button is pressed and holding
         */
        public void RightMouseButtonDown(Vector2 direction)
        {
        }
        

        public void SpaceBar(Vector2 direction)
        {
            throw new System.NotImplementedException();
        }

        public IEquipmentInventoryHandle GetPlayerEquipment()
        {
            return _playerEquipment;
        }

        public IPlayerInventoryHandle GetPlayerInventory()
        {
            return _playerInventory;
        }
    }
}