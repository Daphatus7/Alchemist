using System;
using System.Collections;
using _Script.Attribute;
using _Script.Character.ActionStrategy;
using _Script.Interactable;
using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Items;
using _Script.Places;
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

        private float _facingDirection;
        public float FacingDirection => _facingDirection;
        
        private PlayerInventory _playerInventory;
        public PlayerInventory PlayerInventory => _playerInventory;
        private PlayerEquipmentInventory _playerEquipment;
        public PlayerEquipmentInventory PlayerEquipment => _playerEquipment;

        private InteractionBase _interactionBase;
        private WeaponStrategy _weaponStrategy;
        public WeaponStrategy WeaponStrategy => _weaponStrategy;
        private GenericItemStrategy _genericStrategy;
        public GenericItemStrategy GenericStrategy => _genericStrategy;
        private IActionStrategy _actionStrategy;

        #region player Attribute

        [SerializeField] private float mana = 10f; public float Mana => mana;
        [SerializeField]private float _manaMax = 100f; public float ManaMax => _manaMax;
        [SerializeField] private float stamina= 10f; public float Stamina => stamina;
        [SerializeField]private float _staminaMax = 100f; public float StaminaMax => _staminaMax;
        [SerializeField] private float hunger = 100f; public float Hunger => hunger;
        [SerializeField]private float _hungerMax = 100f; public float HungerMax => _hungerMax;
        
        
        public UnityEvent onStatsChanged = new UnityEvent();
        
        #endregion
        
        [SerializeField] private InventoryUI _inventoryUI;

        #region Player Attribute from Equipment

        private float _attackDamage;
        public float AttackDamage => _attackDamage;


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
                _interactionContext =
                    _interactionBase.InteractableRaycast(transform.position, CursorMovementTracker.CursorPosition);

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

            if (Input.GetKeyDown(KeyCode.I))
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

        private int _gold = 1000; public int Gold => _gold;
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

        #region Stat - Event 

        public UnityEvent GetPlayerHealthUpdateEvent()
        {
            return onHealthChanged;
        }

        public int GetPlayerGold()
        {
            return _gold;
        }

        #endregion

        #region Item 
        
        public bool UseTownScroll(ScrollType spellType, float castTime)
        {
            StartCoroutine(CastSpellCoroutine(spellType, castTime));
            return true;
        }

        private IEnumerator CastSpellCoroutine(ScrollType scrollType, float castTime)
        {
            float remainingTime = castTime;

            while (remainingTime > 0)
            {
                Debug.Log($"Spell '{scrollType}' will be cast in {remainingTime} seconds...");
                yield return new WaitForSeconds(1f);
                remainingTime -= 1f;
            }

            Debug.Log($"Casting spell: {scrollType}");
            switch (scrollType)
            {
                case ScrollType.Town:
                    PlaceManager.Instance.TeleportPlayerToTown(this);
                    break;
                case ScrollType.Dungeon:
                    break;
                default:
                    Debug.Log("Invalid scroll type.");
                    break;
            }
        }
        

        #endregion

        public void EatFood(FoodType foodType, int foodValue)
        {
            switch (foodType)
            {
                case FoodType.Health:
                    Restore(AttributeType.Health, foodValue);
                    break;
                case FoodType.Mana:
                    Restore(AttributeType.Mana, foodValue);
                    break;
                case FoodType.Stamina:
                    Restore(AttributeType.Stamina, foodValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(foodType), foodType, null);
            }
        }
        
        protected override void Restore(AttributeType type, float value)
        {
            switch (type)
            {
                case AttributeType.Health:
                    RestoreHealth(value);
                    break;
                case AttributeType.Mana:
                    RestoreMana(value);
                    break;
                case AttributeType.Stamina:
                    RestoreStamina(value);
                    break;
                case AttributeType.Hunger:
                    RestoreHungry(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
                    break;
            }
            onStatsChanged?.Invoke();
        }
        
        private void RestoreMana(float value)
        {
            mana += value;
            if (mana > 100)
            {
                mana = 100;
            }
            else if (mana < 0)
            {
                mana = 0;
            }
        }
        
        private void RestoreStamina(float value)
        {
            stamina += value;
            if (stamina > 100)
            {
                stamina = 100;
            }
            else if (stamina < 0)
            {
                stamina = 0;
            }
        }
        
        private void RestoreHungry(float value)
        {
            hunger += value;
            if (hunger > 100)
            {
                hunger = 100;
            }
            else if (hunger < 0)
            {
                hunger = 0;
            }
        }
    }
}
