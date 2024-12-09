using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Attribute;
using _Script.Character.ActionStrategy;
using _Script.Interactable;
using _Script.Inventory.EquipmentBackend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.PlayerInventory;
using _Script.Items;
using _Script.Managers;
using _Script.Places;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Character
{
    public class PlayerCharacter : PawnAttribute, IControl, IPlayerUIHandle
    {
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;

        private float _facingDirection;
        public float FacingDirection => _facingDirection;
        
        private PlayerInventory _playerInventory; public PlayerInventory PlayerInventory => _playerInventory;
        private PlayerEquipmentInventory _playerEquipment;
        public PlayerEquipmentInventory PlayerEquipment => _playerEquipment;

        private InteractionBase _interactionBase;

        // Strategies
        private WeaponStrategy _weaponStrategy;
        public WeaponStrategy WeaponStrategy => _weaponStrategy;
        
        private GenericItemStrategy _genericStrategy;
        public GenericItemStrategy GenericStrategy => _genericStrategy;

        // Add torch strategy field
        private TorchItemStrategy _torchStrategy;
        public TorchItemStrategy TorchStrategy => _torchStrategy;

        private IActionStrategy _actionStrategy;
        
        [Header("Player Stats")]
        [SerializeField] private float hungerDamage = 1f;
        [SerializeField] private float hungerRate = -1f;
        [Header("How long it takes for the player to get hungry")]
        [SerializeField] private float hungerDuration = 5f;

        [SerializeField] private float mana = 10f; public float Mana => mana;
        [SerializeField]private float _manaMax = 10f; public float ManaMax => _manaMax;
        [SerializeField] private float stamina= 10f; public float Stamina => stamina;
        [SerializeField]private float _staminaMax = 10f; public float StaminaMax => _staminaMax;
        [SerializeField] private float hunger = 10f; public float Hunger => hunger;
        [SerializeField]private float _hungerMax = 10f; public float HungerMax => _hungerMax;
        [SerializeField] private float _sanity = 10f; public float Sanity => _sanity;
        [SerializeField] private float _sanityMax = 10f; public float SanityMax => _sanityMax;
        
        public UnityEvent onStatsChanged = new UnityEvent();

        [SerializeField] private InventoryUI _inventoryUI;

        private float _attackDamage;
        public float AttackDamage => _attackDamage;

        private int _gold = 1000; public int Gold => _gold;
        private readonly UnityEvent<int> _onGoldChanged = new UnityEvent<int>();

        private InteractionContext _interactionContext;
        private IInteractable _currentlyHighlightedObject = null;

        // For sanity and hunger routines
        private Coroutine _playerSanityRoutine;
        private Coroutine _playerHungerRoutine;
        private bool _isInSafeZone = false; 

        // A dictionary to manage strategies by string keys (optional but helpful if you plan to expand)
        private Dictionary<string, IActionStrategy> _strategies;

        private void Awake()
        {
            _interactionBase = new InteractionBase();
            _weaponStrategy = GetComponent<WeaponStrategy>();
            _genericStrategy = GetComponent<GenericItemStrategy>();

            // Attempt to get torch strategy if you have it as a component
            _torchStrategy = GetComponent<TorchItemStrategy>();

            
            
            InitializePlayerInventories();
            
            


            // Initialize dictionary for strategies
            _strategies = new Dictionary<string, IActionStrategy>();
            if (_weaponStrategy != null) _strategies["Weapon"] = _weaponStrategy;
            if (_genericStrategy != null) _strategies["Generic"] = _genericStrategy;
            if (_torchStrategy != null) _strategies["Torch"] = _torchStrategy;

            UnsetAllStrategy();
        }

        private void Start()
        {
            TimeManager.Instance.onNewDay.AddListener(OnNewDay);
            TimeManager.Instance.onNightStart.AddListener(OnNightStart);
            PauseableUpdate();
        }

        private void PauseableUpdate()
        {
            if (_playerHungerRoutine == null)
            {
                _playerHungerRoutine = StartCoroutine(HungerRoutine());
            }
        }

        private void OnDestroy()
        {
            TimeManager.Instance.onNewDay.RemoveListener(OnNewDay);
            TimeManager.Instance.onNightStart.RemoveListener(OnNightStart);
        }

        public void Update()
        {
            // Interact with world objects
            if (CursorMovementTracker.HasCursorMoved)
            {
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

            if (Input.GetKeyDown(KeyCode.I))
            {
                _inventoryUI.ToggleInventoryUI();
            }
        }

        #region Action Bar - Strategy Pattern

        // Unified enabling/disabling logic
        private void EnableStrategy(IActionStrategy strategyToEnable)
        {
            // Disable all strategies first
            foreach (var strategyPair in _strategies)
            {
                if (strategyPair.Value is MonoBehaviour mb)
                {
                    mb.enabled = false;
                }
            }

            // Enable only the requested strategy
            if (strategyToEnable is MonoBehaviour enableMb)
            {
                enableMb.enabled = true;
            }

            _actionStrategy = strategyToEnable;
        }

        public void SetWeaponStrategy()
        {
            if (_weaponStrategy == null)
            {
                Debug.LogWarning("Weapon strategy not found on PlayerCharacter.");
                return;
            }
            EnableStrategy(_weaponStrategy);
        }

        public void SetGenericStrategy()
        {
            if (_genericStrategy == null)
            {
                Debug.LogWarning("Generic strategy not found on PlayerCharacter.");
                return;
            }
            EnableStrategy(_genericStrategy);
        }

        
        private bool _isTorchActive = false;
        public void SetTorchStrategy()
        {
            if (_torchStrategy == null)
            {
                Debug.LogWarning("Torch strategy not found on PlayerCharacter.");
                return;
            }

            _isTorchActive = true;
            EnableStrategy(_torchStrategy);
        }

        public void UnsetStrategy()
        {
            _isTorchActive = false;
            _actionStrategy = null;
        }

        public void UnsetAllStrategy()
        {
            // Disable all known strategies
            foreach (var strategyPair in _strategies)
            {
                if (strategyPair.Value is MonoBehaviour mb)
                {
                    mb.enabled = false;
                }
            }
            _actionStrategy = null;
        }

        public void LeftMouseButtonDown(Vector2 direction)
        {
            _actionStrategy?.LeftMouseButtonDown(direction);
            _interactionContext?.Interact(gameObject);
        }

        public void LeftMouseButtonUp(Vector2 direction)
        {
            _actionStrategy?.LeftMouseButtonUp(direction);
        }

        public void RightMouseButtonUp(Vector2 direction)
        {
        }

        public void RightMouseButtonDown(Vector2 direction)
        {
        }

        public void Dash(Vector2 direction)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Player Assets

        [SerializeField] private int playerActionbarCapacity = 6;
        
        public void InitializePlayerInventories()
        {
            _playerInventory = new PlayerInventory(this, playerActionbarCapacity);
        }
        
        private PlayerContainer [] _inventories;

        public void AddNewInventory()
        {
            
        }
        
        
        
        

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
                case FoodType.Sanity:
                    Restore(AttributeType.Sanity, foodValue);
                    break;
                case FoodType.Hunger:
                    Restore(AttributeType.Hunger, foodValue);
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
                    AddMana(value);
                    break;
                case AttributeType.Stamina:
                    AddStamina(value);
                    break;
                case AttributeType.Hunger:
                    AddHunger(value);
                    break;
                case AttributeType.Sanity:
                    AddSanity(value);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
            onStatsChanged?.Invoke();
        }

        private void AddMana(float value)
        {
            mana += value;
            if (mana > _manaMax) mana = _manaMax;
            else if (mana < 0) mana = 0;
            onStatsChanged?.Invoke();
        }

        private void AddStamina(float value)
        {
            stamina += value;
            if (stamina > _staminaMax) stamina = _staminaMax;
            else if (stamina < 0) stamina = 0;
            onStatsChanged?.Invoke();
        }

        private void AddSanity(float value)
        {
            Debug.Log($"Adding {value} to sanity.");
            _sanity += value;
            if (_sanity > _sanityMax) _sanity = _sanityMax;
            else if (_sanity < 0) _sanity = 0;
            onStatsChanged?.Invoke();
        }

        private void AddHunger(float value)
        {
            hunger += value;
            if (hunger > _hungerMax)
            {
                hunger = _hungerMax;
            }
            else if (hunger < 0)
            {
                hunger = 0;
                ApplyDamage(hungerDamage);
            }
        }

        public override float ApplyDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                OnDeath();
                onHealthChanged?.Invoke();
                return damage;
            }
            onStatsChanged?.Invoke();
            return damage;
        }

        public void SetInSafeZone(bool isInSafeZone)
        {
            _isInSafeZone = isInSafeZone;
            Debug.Log(_isInSafeZone ? "Player is in safe zone." : "Player is not in safe zone.");
        }

        private void OnNewDay()
        {
            if (_playerSanityRoutine != null)
            {
                StopCoroutine(_playerSanityRoutine);
                _playerSanityRoutine = null;
                Debug.Log("Sanity routine stopped.");
            }
        }

        private void OnNightStart()
        {
            _playerSanityRoutine ??= StartCoroutine(SanityRoutine());
        }

        private IEnumerator SanityRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                if (_isInSafeZone || _isTorchActive) continue;
                AddSanity(-1);
            }
        }

        private IEnumerator HungerRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(hungerDuration);
                AddHunger(-hungerRate);
            }
        }
    }
}
