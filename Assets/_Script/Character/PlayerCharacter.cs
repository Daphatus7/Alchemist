using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Alchemy;
using _Script.Character.ActionStrategy;
using _Script.Character.PlayerRank;
using _Script.Damageable;
using _Script.Interactable;
using _Script.Inventory.ActionBarFrontend;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.PlayerInventory;
using _Script.Items;
using _Script.Managers;
using _Script.Places;
using _Script.Quest;
using _Script.Utilities;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.Events;

namespace _Script.Character
{
    [DefaultExecutionOrder(500)]
    public class PlayerCharacter : MonoBehaviour, IControl, IPlayerUIHandle, IDamageable, IPlayerSave
    {
        #region Inspector Fields & References

        [Header("References")]
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;
        private InventoryManager _inventoryManager;
        private InventoryManager InventoryManager
        {
            get
            {
                if (!_inventoryManager)
                {
                    _inventoryManager = UIManager.Instance.PlayerInventoryUI;
                }
                return _inventoryManager;
            }
        }
        private ActionBarUI _actionBarUI;

        private ActionBarUI ActionBarUI
        {
            get
            {
                if (_actionBarUI == null)
                {
                    _actionBarUI = UIManager.Instance.ActionBarUI;
                }

                return _actionBarUI;
            }
        }
        [SerializeField] private Rigidbody2D _rb;
        [SerializeField] private float cursorDistanceMax = 1f;
        #endregion


        [Header("Movement Settings")]
        private float BaseMoveSpeed => _playerstats.PlayerMovementSpeed.CurrentValue;
        private float SprintMultiplier => _playerstats.PlayerMovementSpeed.SprintMultiplier;
        private float DashSpeed => _playerstats.PlayerMovementSpeed.DashSpeed;
        private float DashDuration => _playerstats.PlayerMovementSpeed.DashDuration;
        private float DashCooldown  => _playerstats.PlayerMovementSpeed.DashCooldown;
        [Tooltip("Time in seconds to smoothly interpolate velocity changes.")]
        [SerializeField] private float velocitySmoothingTime = 0.1f;
        

        private float _facingDirection; public float FacingDirection => _facingDirection;
        private float _attackDamage;

        private bool _isInSafeZone = false;
        private bool _isTorchActive = false;
        private bool _canDash = true;
        private bool _isSprinting = false;
        

        private Coroutine _playerfoodRoutine;

        private readonly UnityEvent<int> _onGoldChanged = new UnityEvent<int>();

        // Smooth movement fieldsd
        private Vector2 _targetVelocity;
        private Vector2 _currentVelocity;
        private float _smoothDampVelocityX;
        private float _smoothDampVelocityY;
        

        #region PlayerRank

        
        private PlayerRank.PlayerRank _playerRank;
        
        public void AddExperience(int exp)
        {
            _playerRank.AddExperience(exp);
        }
        
        public PlayerRank.PlayerRank CurrentRank => _playerRank;
        public NiRank Rank => _playerRank.CurrentRank;

        #endregion

        #region Unity Lifecycle Methods

        private void Awake()
        {
            InitializeBase();
            
            //waiting for generate inventory
        }
        
        
        private void Start()
        {
            
            _playerInventory.SubscribeToInventoryStatus(QuestManager.Instance.OnItemCollected);
            
            _potionEffectManager = GetComponent<PlayerPotionEffectManager>();
            
            PauseableUpdate();
            
            //Add experience to rank
        }
        
        
        /// <summary>
        /// Initialize UI when all data is ready
        /// </summary>
        public void InitializeUI()
        {
            InitializeInventoryUI();
        }

        /// <summary>
        /// Those don't need data initialization
        /// </summary>
        private void InitializeBase()
        {
            _interactionBase = new InteractionBase(cursorDistanceMax);
            _rb = GetComponent<Rigidbody2D>();
            _weaponStrategy = GetComponent<WeaponStrategy>();
            _genericStrategy = GetComponent<GenericItemStrategy>();
            _torchStrategy = GetComponent<TorchItemStrategy>();
            //put strategy in dictionary
            InitializeStrategies();
            UnsetAllStrategy();
            _playerAlchemy = GetComponent<PlayerAlchemy>();
            _playerRank = new PlayerRank.PlayerRank();
            _playerstats.Initialize();
            _playerstats.OnDeath += OnDeath;
        }

 
        
        private IEnumerator AddExperienceRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);
                AddExperience(100);
            } 
        }

        private void OnDestroy()
        {
            if(TimeManager.Instance == null) return;
            _playerstats.OnDeath -= OnDeath;
            _playerInventory.UnsubscribeToInventoryStatus(QuestManager.Instance.OnItemCollected);
            _playerstats.UnsubscribeAll();
        }

        private void Update()
        {
            HandleInteraction();
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;

            // Smoothly adjust velocity towards _targetVelocity for smoother movement
            float smoothTime = velocitySmoothingTime;
            float newVelX = Mathf.SmoothDamp(_rb.linearVelocity.x, _targetVelocity.x, ref _smoothDampVelocityX, smoothTime);
            float newVelY = Mathf.SmoothDamp(_rb.linearVelocity.y, _targetVelocity.y, ref _smoothDampVelocityY, smoothTime);

            _rb.linearVelocity = new Vector2(newVelX, newVelY);
        }

        #endregion

        #region Interaction & Input Handling
        
        private InteractionBase _interactionBase;
        private InteractionContext _interactionContext;
        private IInteractable _currentlyHighlightedObject = null;

        private void PauseableUpdate()
        {
            _playerfoodRoutine ??= StartCoroutine(FoodRoutine());
        }
        
        private void HandleInteraction()
        {
            if (!CursorMovementTracker.HasCursorMoved) return;

            //Interpret the interaction
            _interactionContext = _interactionBase.InteractableFromMouse(transform.position, CursorMovementTracker.CursorPosition);

            //if there is an interactable object
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
                _currentlyHighlightedObject.InteractEnd();
                _currentlyHighlightedObject.OnHighlightEnd();
                _currentlyHighlightedObject = null;
            }
        }

        #endregion

        #region Strategies
        private WeaponStrategy _weaponStrategy; public WeaponStrategy WeaponStrategy => _weaponStrategy;
        private GenericItemStrategy _genericStrategy; public GenericItemStrategy GenericStrategy => _genericStrategy;
        private TorchItemStrategy _torchStrategy; public TorchItemStrategy TorchStrategy => _torchStrategy;
        private IActionStrategy _actionStrategy;
        private Dictionary<string, IActionStrategy> _strategies;
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
            _interactionContext?.Interact(this);
        }

        public void LeftMouseButtonUp(Vector2 direction)
        {
            _actionStrategy?.LeftMouseButtonUp(direction);
    
            // End the current interaction if one is in progress.
            _interactionContext?.StopInteract(this);
        }
        public void RightMouseButtonDown(Vector2 direction) { }
        public void RightMouseButtonUp(Vector2 direction) { }

        #endregion

        #region Movement: Move, Dash, Sprint

        [SerializeField] private int dashCost = 1;
        [SerializeField] private float sprintCost = 0.5f; // per second
        [SerializeField] private float foodRateWhenExhausted = 0.2f; // Food cost per second if stamina not full
        public void Move(Vector2 direction)
        {
            _targetVelocity = direction.normalized * BaseMoveSpeed;
        }

        
        public void Dash(Vector2 direction)
        {
            if (!_canDash || _rb == null) return;
            if(!_playerstats.ConsumeStamina(dashCost)) return;
            StartCoroutine(DashCoroutine(direction));
        }

        public void DashEnd(Vector2 direction)
        {
            // Optional: cleanup logic after dash
        }

        private IEnumerator DashCoroutine(Vector2 direction)
        {
            _canDash = false;

            Vector2 originalTarget = _targetVelocity;
            _targetVelocity = direction.normalized * DashSpeed;

            yield return new WaitForSeconds(DashDuration);

            _targetVelocity = originalTarget;

            DashEnd(direction);

            yield return new WaitForSeconds(DashCooldown);
            _canDash = true;
        }

        private Coroutine _sprintCostRoutine;

        public void Sprint(Vector2 direction)
        {
            return; // Sprinting is disabled for now
            if (_isSprinting) return;
            _isSprinting = true;
            _targetVelocity = direction.normalized * BaseMoveSpeed * SprintMultiplier;

            // Start a coroutine that costs stamina every second
            _sprintCostRoutine = StartCoroutine(SprintCostRoutine());
        }

        public void SprintEnd(Vector2 direction)
        {
            _isSprinting = false;
            _targetVelocity = direction.normalized * BaseMoveSpeed;

            // Stop draining stamina
            if (_sprintCostRoutine != null)
            {
                StopCoroutine(_sprintCostRoutine);
                _sprintCostRoutine = null;
            }
        }

        /// <summary>
        /// Continuously drains stamina while sprinting. Stops when out of stamina or sprint ends.
        /// </summary>
        private IEnumerator SprintCostRoutine()
        {
            while (_isSprinting)
            {
                // Deduct the sprint cost
                _playerstats.ConsumeStamina(sprintCost);
                // Check if we still have stamina
                if (_playerstats.CurrentStamina <= 0)
                {
                    // Not enough stamina, end sprint
                    _isSprinting = false;
                    _targetVelocity = _targetVelocity.normalized * BaseMoveSpeed; // revert to normal speed
                    yield break;
                }

                // Wait one second before next deduction
                yield return new WaitForSeconds(1f);
            }
        }

        #endregion

        #region Player Inventory & Gold
        [SerializeField] private int _gold = 1000; public int Gold => _gold;

        [SerializeField] private int playerActionbarWidth = 6;
        [SerializeField] private int playerActionbarHeight = 2;
        private PlayerInventory _playerInventory; public PlayerInventory PlayerInventory => _playerInventory;
        private PlayerAlchemy _playerAlchemy; public PlayerAlchemy PlayerAlchemy => _playerAlchemy; 

        private void InitializePlayerInventories()
        {
            _playerInventory = new PlayerInventory(this, playerActionbarWidth, playerActionbarHeight);
        }

        private void InitializeInventoryUI()
        {
            ActionBarUI.InitializeInventoryUI(_playerInventory, 0);
            ActionBarUI.ShowUI();
        }

        private void OnDisable()
        {
            //if not playing, then unsubscribe
            if (Application.isPlaying) return;
            _playerInventory.UnsubscribeToInventoryStatus(QuestManager.Instance.OnItemCollected);
            _playerstats.OnDeath -= OnDeath;
        }

        public void OpenContainerInstance(PlayerContainer containerItem)
        {
            InventoryManager.ToggleContainer(containerItem);
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

        #region PlayerStats
        
        
        [Header("Player Stats")]
        [SerializeField] private float foodDamage = 1f;
        [SerializeField] private float foodRate = 1f; public float FoodRate => foodRate;
        [SerializeField] private float foodDuration = 1f;
        
        [SerializeField] private float _staminaMax = 10f; public float StaminaMax => _staminaMax;

        private PlayerPotionEffectManager _potionEffectManager;
        public IPlayerPotionEffectHandler PotionEffectManager => _potionEffectManager;
        
        [SerializeField] private PlayerStatsManager _playerstats = new PlayerStatsManager(); public PlayerStatsManager PlayerStats => _playerstats;
        
        
        public void OnFoodConsumed(FoodEffect foodEffect)
        {
            switch (foodEffect.FoodType)
            {
                case FoodType.Health:
                    _playerstats.AddHealth(foodEffect.Value);
                    break;
                case FoodType.Mana:
                    _playerstats.RestoreMana(foodEffect.Value);
                    break;
                case FoodType.Stamina:
                    _playerstats.RestoreStamina(foodEffect.Value);
                    break;
                case FoodType.Sanity:
                    _playerstats.AddSanity(foodEffect.Value);
                    break;
                case FoodType.Food:
                    _playerstats.RestoreFood(foodEffect.Value);
                    break;
                default:
                    Debug.LogWarning("Invalid food type.");
                    break;
            }
        }
        
        public float ApplyDamage(float damage)
        {
            return _playerstats.TakeDamage(damage);
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
        
        #endregion
        
        #region Day and Night
        

        
        /// <summary>
        /// Reduce player's food over time.
        /// </summary>
        /// <returns></returns>
        private IEnumerator FoodRoutine()
        {
            while (true)
            {
                // For every foodDuration seconds, reduce food by foodRate
                yield return new WaitForSeconds(foodDuration);

                // If stamina is not full, reduce food by (foodRateWhenExhausted + foodRate)
                if (PlayerStats.CurrentStamina < StaminaMax)
                {
                    // If we successfully reduce food (meaning we have enough food to consume),
                    // then restore stamina by 1
                    if (PlayerStats.ConsumeFood(foodRateWhenExhausted + foodRate))
                    {
                        PlayerStats.RestoreStamina(1);
                    }
                }
                // If stamina is full, reduce food by the base foodRate
                else
                {
                    PlayerStats.ConsumeFood(foodRate);
                }
            }
        }

        private void OnDeath()
        {
            //Game Over
            Destroy(this);
        }

        #endregion

        #region Save & Load
        public string SaveKey => "PlayerCharacter";
        public object OnSaveData()
        {
            var inventorySave = _playerInventory.OnSaveData() as PlayerInventorySave;
            var playerStatsSave = _playerstats.OnSave();
            var playerRankSave = _playerRank.OnSave();
            return new PlayerSave(inventorySave, playerStatsSave, _gold, playerRankSave);
        }

        public void OnLoadData(object data)
        {
            if (data is PlayerSave playerSave)
            {
                //Assign gold
                _gold = playerSave.gold;
                //Load inventory
                _playerInventory = new PlayerInventory(this, playerSave.playerInventory);
                //Load stats 
                _playerstats.OnLoad(playerSave.stats);
                //Load rank
                _playerRank.OnLoad(playerSave.rankSave);
            }
            else
            {
                LoadDefaultData();
            }
            InitializeInventoryUI();
        }

        public void LoadDefaultData()
        {
            _gold = 100;
            InitializePlayerInventories();
        }
        #endregion

    
    }
    
    public interface IPlayerSave : ISaveGame
    {
        
    }
}