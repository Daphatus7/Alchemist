using System;
using System.Collections;
using System.Collections.Generic;
using DamageNumbersPro; // for DamageNumber
using UnityEngine;
using _Script.Damageable;
using _Script.Items.AbstractItemTypes;

namespace _Script.Weapon
{
    /**
     * An abstract Weapon: cannot be directly instantiated, 
     * but provides the base structure for cooldown, overlap checks, and damage attempts.
     */
    [RequireComponent(typeof(BoxCollider2D))]
    public abstract class Weapon : MonoBehaviour
    {
        [Header("General Settings")]
        [SerializeField]
        protected float attackCooldown = 1f;
        public float AttackCooldown => attackCooldown;

        protected bool isCoolingDown = false;
        public bool IsCoolingDown => isCoolingDown;

        [Header("Damage Settings")]
        [SerializeField] protected float damage = 1f;  // base damage
        [SerializeField] private List<string> targetTags; // only these tags can be damaged
        protected Collider2D weaponCollider;

        [Tooltip("Prefab or system for displaying floating damage numbers.")]
        [SerializeField] private DamageNumber numberPrefab;

        // Layer mask for "enemies" or valid targets, set as you wish
        private LayerMask enemyLayer = LayerManager.EnemyLayerMask;

        protected ContactFilter2D _filter;
        protected readonly List<Collider2D> _results = new List<Collider2D>();

        public event Action<int> onHitTarget;
        
        /// <summary>
        /// Assign stats from a WeaponItem ScriptableObject (if desired).
        /// </summary>
        public void SetWeaponItem(WeaponItem weaponItem)
        {
            damage = weaponItem.damage;
            attackCooldown = weaponItem.attackSpeed;
        }

        protected virtual void Awake()
        {

            // Prepare the ContactFilter2D so we only detect the chosen layer(s)
            _filter = new ContactFilter2D
            {
                useTriggers = true
            };
            _filter.SetLayerMask(enemyLayer);

            // If not assigned in Inspector, get the local collider
            if (!weaponCollider) 
                weaponCollider = GetComponent<Collider2D>();
        }

        #region Input Handling

        /// <summary>
        /// Called when the player presses the attack button.
        /// Child classes decide how to handle direction or other logic.
        /// </summary>
        public void OnPressed(Vector2 direction)
        {
            Attack(direction);
        }

        /// <summary>
        /// Called when the player releases the attack button (if needed).
        /// Default does nothing.
        /// </summary>
        public virtual void OnReleased(Vector2 direction)
        {
            
        }

        #endregion

        /// <summary>
        /// Attempts an attack if not on cooldown. Child classes can override or extend.
        /// </summary>
        protected virtual void Attack(Vector2 direction)
        {
            
        }

        /// <summary>
        /// 1) Start cooldown
        /// 2) Overlap check on weaponCollider
        /// 3) For each overlap, call TryDamage()
        /// 4) Wait for AttackCooldown
        /// </summary>
        protected abstract IEnumerator AttackRoutine();

        /// <summary>
        /// Called before we do actual damage. Child classes can override to handle conditions
        /// like stamina, or "weapon not ready," etc.
        /// </summary>
        protected virtual bool CanDamage()
        {
            return true;
        }

        /// <summary>
        /// Check if it's a valid target, then pass to OnDamageTarget() to handle actual damage logic.
        /// </summary>
        protected virtual bool TryDamage(Collider2D other)
        {
            if (!CanDamage()) 
                return false;

            if (!IsTarget(other) || !other.TryGetComponent(out IDamageable damageable))
                return false;

            // Now let the child class handle the actual damage formula
            float actualDamage = OnDamageTarget(damageable);
            // Show floating numbers, etc.
            PlayDamageEffect(actualDamage, other);
            OnOnHitTarget((int)actualDamage);
            return true;
        }

        /// <summary>
        /// We define an abstract method to handle the actual "apply damage" logic,
        /// so child classes can specify how the damage is calculated or applied.
        /// Return the final damage inflicted for visual or other use.
        /// </summary>
        protected abstract float OnDamageTarget(IDamageable target);

        /// <summary>
        /// Checks tags or layer to see if 'other' is a valid target.
        /// </summary>
        private bool IsTarget(Collider2D other)
        {
            return targetTags.Contains(other.tag);
        }

        /// <summary>
        /// Spawns floating damage numbers, etc.
        /// </summary>
        protected virtual void PlayDamageEffect(float actualDamage, Collider2D other)
        {
            if (numberPrefab != null && Mathf.Abs(actualDamage) > 0.01f)
            {
                numberPrefab.Spawn(other.transform.position, actualDamage);
            }
        }

        protected virtual void OnOnHitTarget(int damage)
        {
            onHitTarget?.Invoke(damage);
        }
    }
}