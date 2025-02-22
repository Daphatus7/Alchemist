// Author : Peiyu Wang @ Daphatus
// 10 02 2025 02 28

using System.Collections;
using UnityEngine;
using _Script.Utilities;
using _Script.Damageable;
using _Script.Inventory.ItemInstance;
using _Script.Items;

namespace _Script.Weapon
{
    public class MeleeWeapon : Weapon
    {

        private float _attackDistance;
        
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
        /// Deals damage to the target.
        /// </summary>
        protected override float OnDamageTarget(IDamageable target)
        {
            float actualDamage = target.ApplyDamage(Random.Range(damageMin, damageMax));
            return actualDamage;
        }


        public override void SetWeaponItem(WeaponItemInstance weaponItemInstance)
        {
            base.SetWeaponItem(weaponItemInstance);
            _attackDistance = weaponItemInstance.AttackDistance;
        }

        /// <summary>
        /// Always allows damage during the attack window.
        /// </summary>
        protected override bool CanDamage()
        {
            return true;
        }
        
        /// <summary>
        /// Called when the user presses the attack button.
        /// </summary>
        protected override void Attack(Vector2 direction)
        {
            if (isCoolingDown) return;

            StartCoroutine(AttackRoutine());
            
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            SetAttackingDirection(direction);

            // Animate the weapon movement and rotation.
            StartCoroutine(UpdateMovementCoroutine(direction));
            StartCoroutine(AttackCooldownCoroutine());
        }
        
        protected override IEnumerator AttackRoutine()
        {
            isCoolingDown = true;
            _isAttacking = true;
    
            float t = 0f;
            bool damageApplied = false;
            float threshold = 0.7f; // Damage occurs when curve reaches 70% of its maximum value.
    
            // Continue until the attack duration is reached.
            while (t < AttackTime)
            {
                t += Time.deltaTime;
                float progress = t / AttackTime;
                float curveValue = animationCurve.Evaluate(progress);
        
                // Once the curve reaches the threshold and damage hasn't been applied yet...
                if (!damageApplied && curveValue >= threshold)
                {
                    _results.Clear();
                    int count = Physics2D.OverlapCollider(weaponCollider, _filter, _results);
                    if (count > 0)
                    {
                        foreach (var hit in _results)
                        {
                            if (TryDamage(hit))
                            {
                                damageApplied = true;
                                break;
                            }
                        }
                    }
                }
        
                yield return null;
            }
    
            // Wait for any additional cooldown time after the attack.
            yield return new WaitForSeconds(AttackCooldown);
    
            _isAttacking = false;
            isCoolingDown = false;
        }

        private void SetAttackingDirection(Vector2 direction)
        {
            float angle = Vector2.SignedAngle(Vector2.right, direction);
            _attackingLeft = Helper.IsFaceLeft(angle);
        }

        /// <summary>
        /// Animates the weapon's movement and rotation over the attack duration.
        /// </summary>
        protected virtual IEnumerator UpdateMovementCoroutine(Vector2 direction)
        {
            float time = 0f;

            while (time < AttackTime)
            {
                time += Time.deltaTime;
                UpdatePosition(time, direction);
                UpdateRotation(time);
                yield return null;
            }

            _isAttacking = false;
        }

        protected virtual void UpdatePosition(float time, Vector2 direction)
        {
            Vector3 forward = ForwardMovement(time, direction);
            Vector3 right   = RightMovement(time, direction);
            transform.position = forward + right;
        }

        /// <summary>
        /// Updates the weapon rotation based on the selected attack form.
        /// </summary>
        protected virtual void UpdateRotation(float time)
        {
            switch (AttackForm)
            {
                case AttackForm.Hammer:
                    HammerRotation(time);
                    break;
                case AttackForm.Stab:
                    Stab(time);
                    break;
                case AttackForm.Slash:
                    Slash(time);
                    break;
                default:
                    HammerRotation(time);
                    break;
            }
        }

        protected virtual Vector3 ForwardMovement(float time, Vector2 direction)
        {
            float curveValue = animationCurve.Evaluate(time / AttackTime);
            Vector3 movement = direction * (_attackDistance * curveValue);
            return transform.root.position + movement;
        }

        protected virtual Vector3 RightMovement(float time, Vector2 direction)
        {
            // Implement sideways movement if desired.
            return Vector3.zero;
        }

        private IEnumerator AttackCooldownCoroutine()
        {
            isCoolingDown = true;
            yield return new WaitForSeconds(AttackCooldown);
            isCoolingDown = false;
        }

        private void HammerRotation(float time)
        {
            if (AttackingLeft)
            {
                float angle = Mathf.Lerp(-45, 45, animationCurve.Evaluate(time / AttackTime));
                transform.rotation = Quaternion.Euler(0, 0, angle) * InitialRotation;
            }
            else
            {
                float angle = Mathf.Lerp(45, -45, animationCurve.Evaluate(time / AttackTime));
                transform.rotation = Quaternion.Euler(0, 0, angle) * InitialRotation;
            }
        }

        /// <summary>
        /// For a stab attack, the weapon makes a quick forward thrust with a slight tilt.
        /// </summary>
        private void Stab(float time)
        {
            // Define a small tilt angle.
            float stabAngle = 10f;
            float t = time / AttackTime;
            float angle;
            // Use a two-phase motion: tilt forward then recover.
            if (t <= 0.5f)
            {
                angle = Mathf.Lerp(0, AttackingLeft ? -stabAngle : stabAngle, t * 2);
            }
            else
            {
                angle = Mathf.Lerp(AttackingLeft ? -stabAngle : stabAngle, 0, (t - 0.5f) * 2);
            }
            transform.rotation = Quaternion.Euler(0, 0, angle) * InitialRotation;
        }

        /// <summary>
        /// For a slash attack, the weapon swings through a wide arc.
        /// </summary>
        private void Slash(float time)
        {
            // Define a wide sweeping arc.
            float slashStart = AttackingLeft ? -90f : 90f;
            float slashEnd   = AttackingLeft ? 90f : -90f;
            float angle = Mathf.Lerp(slashStart, slashEnd, animationCurve.Evaluate(time / AttackTime));
            transform.rotation = Quaternion.Euler(0, 0, angle) * InitialRotation;
        }
    }
}