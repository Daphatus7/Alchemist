using System.Collections;
using _Script.Utilities;
using UnityEngine;

namespace _Script.Movement
{
    public class Attack : MonoBehaviour
    {
        [SerializeField] private float attackTime = 0.5f; public float AttackTime => attackTime;
        [SerializeField] private float attackDistance = 3f; public float AttackDistance => attackDistance;
        [SerializeField] private AnimationCurve animationCurve; public AnimationCurve AnimationCurve => animationCurve;
        
        //initial position and rotation
        private Vector3 _initialPosition; public Vector3 InitialPosition => _initialPosition;
        private Quaternion _initialRotation; public Quaternion InitialRotation => _initialRotation;
        
        private bool _attackingLeft = false; public bool AttackingLeft => _attackingLeft;
        
        public void OnEnable()
        {
            _initialPosition = transform.position;
            _initialRotation = transform.rotation;
            SetAttackingDirection();
            StartCoroutine(UpdateMovementCoroutine());
        }
        
        private void SetAttackingDirection()
        {
            _attackingLeft = Helper.IsFaceLeft(transform.rotation.eulerAngles.z);
        }

        protected virtual IEnumerator UpdateMovementCoroutine()
        {

            float time = 0;
            while (time < attackTime)
            {
                time += Time.deltaTime;
                UpdatePosition(time);
                UpdateRotation(time);
                //add relative displacement
                yield return null;
            }
            Destroy(gameObject);
        }
        
        protected virtual void UpdatePosition(float time)
        {
            transform.position = ForwardMovement(time) + RightMovement(time);
        }
        
        protected virtual void UpdateRotation(float time)
        {
        }

        protected virtual Vector3 ForwardMovement(float time)
        {
            return transform.right * (attackDistance * animationCurve.Evaluate(time / attackTime))+ transform.root.position;
        }
        
        protected virtual Vector3 RightMovement(float time)
        {
            return Vector3.zero;
        }
    }
}