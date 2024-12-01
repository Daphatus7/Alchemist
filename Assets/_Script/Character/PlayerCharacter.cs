using System.Collections.Generic;
using _Script.Attribute;
using _Script.Character.Ability;
using _Script.Character.ActionStrategy;
using _Script.Interactable;
using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryHandles;
using _Script.Utilities;
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
        
        private InteractionBase _interactionBase;
        private WeaponStrategy _weaponStrategy; public WeaponStrategy WeaponStrategy => _weaponStrategy;
        private GenericItemStrategy _genericStrategy; public GenericItemStrategy GenericStrategy => _genericStrategy;
        private IActionStrategy _actionStrategy;

        #region Player Attribute from Equipment
        
        private float _attackDamage; public float AttackDamage => _attackDamage;
        
        
        public void DebugStat()
        {
        }
        
        #endregion

        private void Awake()
        {
            _interactionBase = new InteractionBase();
            _weaponStrategy = GetComponent<WeaponStrategy>();
            _genericStrategy = GetComponent<GenericItemStrategy>();
            _playerInventory = GetComponentInChildren<PlayerInventory>();
            _playerEquipment = GetComponent<PlayerEquipmentInventory>();
            
            UnsetAllStrategy();
        }
        
        private InteractionContext _context;
        private IInteractable _currentlyHighlightedObject = null;

        public void Update()
        {
            //Interact with world objects
            if (CursorMovementTracker.HasCursorMoved)
            {
                //get the interactable object
                _context = _interactionBase.InteractableRaycast(transform.position, CursorMovementTracker.CursorPosition);

                if (_context != null)
                {
                    _context.GetInteractableName();

                    _context.Highlight(out var interactable);

                    if (_currentlyHighlightedObject == interactable) return;
                    _currentlyHighlightedObject?.OnHighlightEnd();
                    _currentlyHighlightedObject = interactable;
                }
                else
                {
                    // if there is no interactable object
                    if (_currentlyHighlightedObject == null) return;
                    _currentlyHighlightedObject.OnHighlightEnd();
                    _currentlyHighlightedObject = null;
                }
            }
        }

        
        #region Action Bar - Strategy Pattern
        
        public void SetWeaponStrategy()
        {
            //Disable the generic strategy
            _genericStrategy.enabled = false;
            
            //Enable the weapon strategy
            _weaponStrategy.enabled = true;
            
            
            _actionStrategy = _weaponStrategy;
        }
        
        public void SetGenericStrategy()
        {
            //Disable the weapon strategy
            _weaponStrategy.enabled = false;
            
            //Enable the generic strategy
            _genericStrategy.enabled = true;
            
            _actionStrategy = _genericStrategy;
        }
        
        public void UnsetStrategy()
        {
            _actionStrategy = null;
        }

        public void UnsetAllStrategy()
        {
            _actionStrategy = null;
            _weaponStrategy.enabled = false;
            _genericStrategy.enabled = false;
        }
        
        /**
         * Called when the left mouse button is pressed and holding
         */
        public void LeftMouseButtonDown(Vector2 direction)
        {
            //Get Action bar Item
            //call the function
            
            if(_context != null)
            {
                _context.Interact(gameObject);
            }
            else
            {
                _actionStrategy?.LeftMouseButtonDown(direction);
            }
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