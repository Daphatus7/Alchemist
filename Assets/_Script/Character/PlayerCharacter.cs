using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Attribute;
using _Script.Character.ActionStrategy;
using _Script.Interactable;
using _Script.Inventory.ActionBarFrontend;
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
        [Header("References")]
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;
        [SerializeField] private InventoryManager _inventoryManager;
        [SerializeField] private ActionBarUI _actionBarUI;
        [SerializeField] private Rigidbody2D _rb;

        [Header("Player Stats")]
        [SerializeField] private float hungerDamage = 1f;
        [SerializeField] private float hungerRate = -1f;
        [SerializeField] private float hungerDuration = 5f;

        [SerializeField] private float mana = 10f;        public float Mana => mana;
        [SerializeField] private float _manaMax = 10f;    public float ManaMax => _manaMax;
        [SerializeField] private float stamina = 10f;     public float Stamina => stamina;
        [SerializeField] private float _staminaMax = 10f; public float StaminaMax => _staminaMax;
        [SerializeField] private float hunger = 10f;      public float Hunger => hunger;
        [SerializeField] private float _hungerMax = 10f;  public float HungerMax => _hungerMax;
        [SerializeField] private float _sanity = 10f;     public float Sanity => _sanity;
        [SerializeField] private float _sanityMax = 10f;  public float SanityMax => _sanityMax;

        [Header("Movement Settings")]
        [SerializeField] private float baseMoveSpeed = 5f;
        [SerializeField] private float sprintMultiplier = 1.5f;
        [SerializeField] private float dashSpeed = 10f;
        [SerializeField] private float dashDuration = 0.2f;
        [SerializeField] private float dashCooldown = 2f;

        public UnityEvent onStatsChanged = new UnityEvent();

        private float _facingDirection; public float FacingDirection => _facingDirection;
        private float _attackDamage; public float AttackDamage => _attackDamage;
        private int _gold = 1000; public int Gold => _gold;

        private bool _isInSafeZone = false;
        private bool _isTorchActive = false;
        private bool _canDash = true;
        private bool _isSprinting = false;

        private InteractionBase _interactionBase;
        private InteractionContext _interactionContext;
        private IInteractable _currentlyHighlightedObject = null;

        private PlayerInventory _playerInventory; public PlayerInventory PlayerInventory => _playerInventory;
        private PlayerEquipmentInventory _playerEquipment; public PlayerEquipmentInventory PlayerEquipment => _playerEquipment;

        private WeaponStrategy _weaponStrategy; public WeaponStrategy WeaponStrategy => _weaponStrategy;
        private GenericItemStrategy _genericStrategy; public GenericItemStrategy GenericStrategy => _genericStrategy;
        private TorchItemStrategy _torchStrategy; public TorchItemStrategy TorchStrategy => _torchStrategy;

        private IActionStrategy _actionStrategy;
        private Dictionary<string, IActionStrategy> _strategies;
        
        private Coroutine _playerSanityRoutine;
        private Coroutine _playerHungerRoutine;
        
        private readonly UnityEvent<int> _onGoldChanged = new UnityEvent<int>();

        private void Awake()
        {
            _interactionBase = new InteractionBase();

            _weaponStrategy = GetComponent<WeaponStrategy>();
            _genericStrategy = GetComponent<GenericItemStrategy>();
            _torchStrategy = GetComponent<TorchItemStrategy>();
            _rb = GetComponent<Rigidbody2D>();

            InitializePlayerInventories();
            InitializeStrategies();
            
            UnsetAllStrategy();
        }

        private void Start()
        {
            TimeManager.Instance.onNewDay.AddListener(OnNewDay);
            TimeManager.Instance.onNightStart.AddListener(OnNightStart);

            PauseableUpdate();
        }

        private void OnDestroy()
        {
            TimeManager.Instance.onNewDay.RemoveListener(OnNewDay);
            TimeManager.Instance.onNightStart.RemoveListener(OnNightStart);
        }

        private void Update()
        {
            HandleInteraction();
        }

        private void PauseableUpdate()
        {
            if (_playerHungerRoutine == null)
            {
                _playerHungerRoutine = StartCoroutine(HungerRoutine());
            }
        }

        #region Interaction & Input Handling

        private void HandleInteraction()
        {
            if (!CursorMovementTracker.HasCursorMoved) return;

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
                if (_currentlyHighlightedObject == null) return;
                _currentlyHighlightedObject.OnHighlightEnd();
                _currentlyHighlightedObject = null;
            }
        }

        #endregion

        #region Strategies (Action Bar)

        private void InitializeStrategies()
        {
            _strategies = new Dictionary<string, IActionStrategy>();
            if (_weaponStrategy != null) _strategies["Weapon"] = _weaponStrategy;
            if (_genericStrategy != null) _strategies["Generic"] = _genericStrategy;
            if (_torchStrategy != null) _strategies["Torch"] = _torchStrategy;
        }

        private void EnableStrategy(IActionStrategy strategyToEnable)
        {
            foreach (var strategyPair in _strategies)
            {
                if (strategyPair.Value is MonoBehaviour mb)
                    mb.enabled = false;
            }

            if (strategyToEnable is MonoBehaviour enableMb)
                enableMb.enabled = true;

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
            foreach (var strategyPair in _strategies)
            {
                if (strategyPair.Value is MonoBehaviour mb)
                    mb.enabled = false;
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

        public void RightMouseButtonDown(Vector2 direction) { }
        public void RightMouseButtonUp(Vector2 direction) { }

        #endregion

        #region Movement: Move, Dash, Sprint

        /// <summary>
        /// Move the player in the given direction at base speed.
        /// Call this function whenever you have updated movement input.
        /// </summary>
        public void Move(Vector2 direction)
        {
            if (_rb == null) return;
            _rb.linearVelocity = direction.normalized * baseMoveSpeed;
        }

        /// <summary>
        /// Start a dash in the given direction. This sets a temporary velocity and restores after dashDuration.
        /// </summary>
        public void Dash(Vector2 direction)
        {
            if (!_canDash || _rb == null) return;
            StartCoroutine(DashCoroutine(direction));
        }

        /// <summary>
        /// Called after the dash ends. Use this if you need cleanup logic after dash.
        /// </summary>
        public void DashEnd(Vector2 direction)
        {
            // Optional: cleanup logic after dash
        }

        private IEnumerator DashCoroutine(Vector2 direction)
        {
            _canDash = false;

            // Store original velocity
            Vector2 originalVelocity = _rb.linearVelocity;

            // Set dash velocity
            _rb.linearVelocity = direction.normalized * dashSpeed;

            // Wait for dashDuration
            yield return new WaitForSeconds(dashDuration);

            // Restore original velocity
            _rb.linearVelocity = originalVelocity;

            DashEnd(direction);

            // Wait for cooldown
            yield return new WaitForSeconds(dashCooldown);
            _canDash = true;
        }

        /// <summary>
        /// Begin sprinting, increasing speed while maintaining the given direction.
        /// Call Move after this if direction changes.
        /// </summary>
        public void Sprint(Vector2 direction)
        {
            if (_isSprinting) return;
            _isSprinting = true;
            if (_rb != null)
                _rb.linearVelocity = direction.normalized * baseMoveSpeed * sprintMultiplier;
        }

        /// <summary>
        /// Stop sprinting, returning to base speed.
        /// </summary>
        public void SprintEnd(Vector2 direction)
        {
            _isSprinting = false;
            if (_rb != null)
                _rb.linearVelocity = direction.normalized * baseMoveSpeed;
        }

        #endregion

        #region Player Inventory & Gold

        [SerializeField] private int playerActionbarCapacity = 6;

        private void InitializePlayerInventories()
        {
            _playerInventory = new PlayerInventory(this, playerActionbarCapacity);
            _actionBarUI.InitializeInventoryUI(_playerInventory, playerActionbarCapacity, 0);
        }
        
        public void OpenContainerInstance(PlayerContainer containerItem)
        {
            _inventoryManager.ToggleContainer(containerItem);
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

        #region Stats & Events

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

        #endregion

        #region Item & Consumables

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

        #endregion

        #region Sanity & Hunger Routines

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

        #endregion
    }
}