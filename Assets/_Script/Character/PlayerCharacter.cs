using _Script.Attribute;
using _Script.Character.Ability;
using _Script.Character.ActionStrategy;
using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryHandles;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    public class PlayerCharacter : PawnAttribute, IControl, IPlayerInventoryHandler, IPlayerUIHandle
    {
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;
        
        
        private float _facingDirection; public float FacingDirection => _facingDirection;

        private IPlayerInventoryHandle _playerInventory;
        private IPlayerEquipmentHandle _playerEquipment;
        
        private WeaponStrategy _weaponStrategy; public WeaponStrategy WeaponStrategy => _weaponStrategy;
        private GenericStrategy _genericStrategy; public GenericStrategy GenericStrategy => _genericStrategy;
        private IActionStrategy _actionStrategy;

        #region Player Attribute from Equipment
        
        private float _attackDamage; public float AttackDamage => _attackDamage;
        
        
        public void DebugStat()
        {
        }
        
        #endregion

        private void Awake()
        {
            _weaponStrategy = GetComponent<WeaponStrategy>();
            _genericStrategy = GetComponent<GenericStrategy>();
            _playerInventory = GetComponentInChildren<PlayerInventory>();
            _playerEquipment = GetComponent<PlayerEquipmentInventory>();
            //debug Equipment inventory
        }
        
        #region Action Bar - Strategy Pattern
        

        
        public void SetWeaponStrategy()
        {
            _actionStrategy = _weaponStrategy;
        }
        
        public void SetGenericStrategy()
        {
            _actionStrategy = _genericStrategy;
        }
        
        public void UnsetStrategy()
        {
            _actionStrategy = null;
        }

        /**
         * Called when the left mouse button is pressed and holding
         */
        public void LeftMouseButtonDown(Vector2 direction)
        {
            //Get Action bar Item
            //call the function
            _actionStrategy?.LeftMouseButtonDown(direction);
        }
        
        /**
         * Release the right mouse button
         */
        public void LeftMouseButtonUp(Vector2 direction)
        {
            _actionStrategy?.LeftMouseButtonUp(direction);
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
        
        public void Dash(Vector2 direction)
        {
            throw new System.NotImplementedException();
        }

        #endregion
        
        #region Inventory Handle

        public IPlayerEquipmentHandle GetPlayerEquipment()
        {
            return _playerEquipment;
        }

        public IPlayerInventoryHandle GetPlayerInventory()
        {
            return _playerInventory;
        }

        #endregion
        
        #region Stat

        public UnityEvent GetPlayerHealthUpdateEvent()
        {
            return onHealthChanged;
        }

        public float GetPlayerHealth()
        {
            return Health;
        }

        public float GetPlayerMaxHealth()
        {
            return HealthMax;
        }

        #endregion
    }
}