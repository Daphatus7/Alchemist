using System;
using _Script.Attribute;
using _Script.Character.ActionStrategy;
using _Script.Interactable;
using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    public class PlayerCharacter : PawnAttribute, IControl, IPlayerUIHandle
    {
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;
        
        private float _facingDirection; public float FacingDirection => _facingDirection;

        private PlayerInventory _playerInventory; public PlayerInventory PlayerInventory => _playerInventory;
        private PlayerEquipmentInventory _playerEquipment; public PlayerEquipmentInventory PlayerEquipment => _playerEquipment;
        
        private InteractionBase _interactionBase;
        private WeaponStrategy _weaponStrategy; public WeaponStrategy WeaponStrategy => _weaponStrategy;
        private GenericItemStrategy _genericStrategy; public GenericItemStrategy GenericStrategy => _genericStrategy;
        private IActionStrategy _actionStrategy;
        
        [SerializeField] private InventoryUI _inventoryUI;

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
        
        private InteractionContext _interactionContext;
        private IInteractable _currentlyHighlightedObject = null;

        public void Update()
        {
            //Interact with world objects
            if (CursorMovementTracker.HasCursorMoved)
            {
                //get the interactable object
                _interactionContext = _interactionBase.InteractableRaycast(transform.position, CursorMovementTracker.CursorPosition);

                if (_interactionContext != null)
                {
                    _interactionContext.GetInteractableName();
                    
                    _interactionContext.Highlight(out var interactable);

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
            
            if(Input.GetKeyDown(KeyCode.I))
            {
                _inventoryUI.ToggleInventoryUI();
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
            _actionStrategy?.LeftMouseButtonDown(direction);
            _interactionContext?.Interact(gameObject);
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
        #region Player Assets

        private int _gold = 1000;
        private readonly UnityEvent<int> _onGoldChanged = new UnityEvent<int>();
        public void AddGold(int amount)
        {
            _gold += amount;
            _onGoldChanged?.Invoke(_gold);
        }
        
        public bool RemoveGold(int amount)
        {
            if (_gold - amount < 0) return false;
            _gold -= amount;
            _onGoldChanged?.Invoke(_gold);
            return true;
        }
        
        public UnityEvent<int> PlayerGoldUpdateEvent()
        {
            return _onGoldChanged;
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

        public int GetPlayerGold()
        {
            return _gold;
        }

        #endregion
    }
}