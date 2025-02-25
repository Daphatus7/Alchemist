// Author : Peiyu Wang @ Daphatus
// 25 02 2025 02 36

using _Script.Managers;
using UnityEngine;

namespace _Script.Character
{
    public class PlayerCamera : MonoBehaviour
    {
        private Transform _target;
        public Transform Target
        {
            get
            {
                if (!_target)
                {
                    _target = GameManager.Instance.PlayerCharacter.transform;
                }
                return _target;
            }
        }
        [Tooltip("Offset from the target's position.")]
        public Vector3 offset = new Vector3(0, 0, -9f);
        [Tooltip("How quickly the camera follows the target.")]
        public float followSpeed = 5f;
        
        [Header("Screen Shake Settings")]
        [Tooltip("Default damping speed for the screen shake effect.")]
        public float shakeDampingSpeed = 1.0f;
        [Tooltip("Default shake magnitude.")]
        public float defaultShakeMagnitude = 0.2f;
        
        // Private variables for smooth following
        private Vector3 velocity = Vector3.zero;
        
        // Variables for screen shake
        private float shakeDuration = 0f;
        private float shakeMagnitude = 0f;
        
        // Original position before applying shake (relative to offset)
        private Vector3 basePosition;

        private void Start()
        {
            if (Target == null)
            {
                Debug.LogError("PlayerCamera: No target assigned. Please assign a target (player) to follow.");
            }
            // Set the base position based on the target's initial position and offset.
            basePosition = Target.position + offset;
        }

        private void LateUpdate()
        {
            if (Target)
            {
                // Calculate target position
                basePosition = Target.position + offset;
                // Compute smoothing factor using an exponential decay
                float t = 1 - Mathf.Exp(-followSpeed * Time.deltaTime);
                // Smoothly move the camera using Lerp with a constant factor t
                transform.position = Vector3.Lerp(transform.position, basePosition, t);
            }

            // Apply screen shake if active
            if (shakeDuration > 0)
            {
                Vector3 shakeOffset = new Vector3(Random.Range(-1f, 1f),
                    Random.Range(-1f, 1f),
                    0f) * shakeMagnitude;
                transform.position += shakeOffset;
                shakeDuration -= Time.deltaTime * shakeDampingSpeed;
            }
        }

        /// <summary>
        /// Triggers a screen shake effect.
        /// </summary>
        /// <param name="duration">Duration of the shake in seconds.</param>
        /// <param name="magnitude">Magnitude of the shake (how far the camera will move).</param>
        public void ShakeCamera(float duration, float magnitude)
        {
            shakeDuration = duration;
            shakeMagnitude = magnitude;
        }
        
        /// <summary>
        /// Overloaded method to trigger screen shake with a default magnitude.
        /// </summary>
        /// <param name="duration">Duration of the shake in seconds.</param>
        public void ShakeCamera(float duration)
        {
            ShakeCamera(duration, defaultShakeMagnitude);
        }
    }
}