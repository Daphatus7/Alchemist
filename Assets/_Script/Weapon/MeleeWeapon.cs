using System;
using System.Collections;
using _Script.Utilities;
using UnityEngine;

namespace _Script.Weapon
{

    public class MeleeWeapon : Weapon
    {
        
        [SerializeField] private float attackTime = 0.5f; public float AttackTime => attackTime;
        [SerializeField] private float attackDistance = 3f; public float AttackDistance => attackDistance;
        [SerializeField] private AnimationCurve animationCurve; public AnimationCurve AnimationCurve => animationCurve;
        
        //initial position and rotation
        private Vector3 _initialPosition; public Vector3 InitialPosition => _initialPosition;
        private Quaternion _initialRotation; public Quaternion InitialRotation => _initialRotation;
        private bool _attackingLeft = false;
        protected bool AttackingLeft => _attackingLeft;

        private bool _isAttacking = false;
        
        protected override bool CanDamage()
        {
            return _isAttacking;
        }

        protected override void OnTriggerStay2D(Collider2D other)
        {
            base.OnTriggerStay2D(other);
            _isAttacking = false;
        }


        protected override void Attack(Vector2 direction)
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            SetAttackingDirection(direction);
            StartCoroutine(UpdateMovementCoroutine(direction));
            StartCoroutine(AttackCooldownCoroutine());
        }

        private void SetAttackingDirection(Vector2 direction)
        {
            var angle = Vector2.SignedAngle(Vector2.right, direction);
            _attackingLeft = Helper.IsFaceLeft(angle);
        }

        protected virtual IEnumerator UpdateMovementCoroutine(Vector2 direction)
        {
            float time = 0;
            _isAttacking = true;
            while (time < attackTime)
            {
                time += Time.deltaTime;
                UpdatePosition(time, direction);
                UpdateRotation(time);
                //add relative displacement
                yield return null;
            }
            _isAttacking = false;
        }
        
        protected virtual void UpdatePosition(float time, Vector2 direction)
        {
            transform.position = ForwardMovement(time, direction) + RightMovement(time, direction);
        }
        
        protected virtual void UpdateRotation(float time)
        {
        }
        
        private IEnumerator AttackCooldownCoroutine()
        {
            isCoolingDown = true;
            yield return new WaitForSeconds(AttackCooldown);
            isCoolingDown = false;
        }

        protected virtual Vector3 ForwardMovement(float time, Vector2 direction)
        {
            Vector3 vector3 = direction;
            return vector3 * (attackDistance * animationCurve.Evaluate(time / attackTime)) + transform.root.position;
        }
        
        protected virtual Vector3 RightMovement(float time, Vector2 direction)
        {
            return Vector3.zero;
        }
    }
}