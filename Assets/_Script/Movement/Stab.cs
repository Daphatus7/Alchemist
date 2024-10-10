using System.Collections;
using UnityEngine;

namespace _Script.Movement
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Stab : MonoBehaviour
    {
        [SerializeField] private float attackTime = 0.5f;
        [SerializeField]private AnimationCurve _animationCurve;
        
        private void OnEnable()
        {
            StartCoroutine(UpdateMovementCoroutine());
        }

        private IEnumerator UpdateMovementCoroutine()
        {

            float time = 0;
            var initialPosition = transform.position;
            while (time < attackTime)
            {
                time += Time.deltaTime;
                transform.position = _animationCurve.Evaluate(time/attackTime) * transform.right + initialPosition;
                yield return null;
            }
            Destroy(gameObject);
        }
        

        
    }
}