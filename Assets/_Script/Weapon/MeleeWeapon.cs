using System.Collections;
using UnityEngine;
using _Script.Utilities;
using _Script.Damageable;

namespace _Script.Weapon
{
    public class MeleeWeapon : Weapon
    {
        [SerializeField] private float attackTime = 0.5f; 
        public float AttackTime => attackTime;
        
        [SerializeField] private float attackDistance = 3f;
        public float AttackDistance => attackDistance;
        
        [SerializeField] private AnimationCurve animationCurve;
        public AnimationCurve AnimationCurve => animationCurve;
        
        private Vector3 _initialPosition; 
        public Vector3 InitialPosition => _initialPosition;
        
        private Quaternion _initialRotation; 
        public Quaternion InitialRotation => _initialRotation;

        private bool _attackingLeft = false;
        protected bool AttackingLeft => _attackingLeft;

        private bool _isAttacking = false;

        /// <summary>
        /// We override the abstract method from Weapon:
        /// specifically how we deal damage to the target.
        /// </summary>
        protected override float OnDamageTarget(IDamageable target)
        {
            // Simple example: apply the parent's 'damage' directly
            // Maybe you do some random multiplier or critical chance, etc.
            float actualDamage = target.ApplyDamage(damage);
            return actualDamage;
        }

        /// <summary>
        /// We only want to deal damage while in the "attack" window.
        /// So if _isAttacking is false, we skip damage.
        /// </summary>
        protected override bool CanDamage()
        {
            return true;
        }
        
        /// <summary>
        /// Called when the user presses attack. 
        /// We do normal Weapon attack logic and also start our melee animations.
        /// </summary>
        protected override void Attack(Vector2 direction)
        {
            // 1) Call base Attack (starts overlap check + cooldown routine)
            if (isCoolingDown) return;

            StartCoroutine(AttackRoutine());
            
            // 2) Our melee-specific logic
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            SetAttackingDirection(direction);

            // Animate the weapon
            StartCoroutine(UpdateMovementCoroutine(direction));

            // Optional: If we want a separate "AttackCooldownCoroutine" for movement
            StartCoroutine(AttackCooldownCoroutine());
        }
        
        protected override IEnumerator AttackRoutine()
        {
            isCoolingDown = true;
            _isAttacking = true;
            
            _results.Clear();
            int count = Physics2D.OverlapCollider(weaponCollider, _filter, _results);
            if (count > 0)
            {
                foreach (var hit in _results)
                {
                    if(TryDamage(hit))
                        break; 
                }
            }

            yield return new WaitForSeconds(attackCooldown);
            _isAttacking = false;
            isCoolingDown = false;
        }

        private void SetAttackingDirection(Vector2 direction)
        {
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            _attackingLeft = Helper.IsFaceLeft(angle);
        }

        /// <summary>
        /// Animate the weapon over 'attackTime' using the given curve.
        /// </summary>
        protected virtual IEnumerator UpdateMovementCoroutine(Vector2 direction)
        {
            float time = 0f;

            while (time < attackTime)
            {
                time += Time.deltaTime;
                UpdatePosition(time, direction);
                UpdateRotation(time);
                yield return null;
            }

            // Attack motion done
            _isAttacking = false;
        }

        protected virtual void UpdatePosition(float time, Vector2 direction)
        {
            Vector3 forward = ForwardMovement(time, direction);
            Vector3 right   = RightMovement(time, direction);
            transform.position = forward + right;
        }

        protected virtual void UpdateRotation(float time)
        {
            // e.g., rotate the weapon if you want a slash arc
        }

        /// <summary>
        /// A separate cooldown routine if needed; the base weapon also does an internal cooldown. 
        /// If you want them combined, you can remove this or unify them.
        /// </summary>
        private IEnumerator AttackCooldownCoroutine()
        {
            isCoolingDown = true;
            yield return new WaitForSeconds(AttackCooldown);
            isCoolingDown = false;
        }

        protected virtual Vector3 ForwardMovement(float time, Vector2 direction)
        {
            float curveValue = animationCurve.Evaluate(time / attackTime);
            // Move from the transform.root position in the given direction
            Vector3 movement = direction * (attackDistance * curveValue);
            return transform.root.position + movement;
        }

        protected virtual Vector3 RightMovement(float time, Vector2 direction)
        {
            // If you want a sideways arc, implement it here
            return Vector3.zero;
        }
    }
}