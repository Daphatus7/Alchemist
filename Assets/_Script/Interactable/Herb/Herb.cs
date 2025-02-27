// Author : Peiyu Wang @ Daphatus
// 26 02 2025 02 29

using System.Collections;
using _Script.Character;
using _Script.Drop;
using UnityEngine;

namespace _Script.Interactable.Herb
{
    [RequireComponent(typeof(Collider2D))]
    public class Herb : MonoBehaviour, IInteractable
    {
        [Header("Gathering Settings")]
        [Tooltip("Time (in seconds) required to gather this herb.")]
        [SerializeField] private float _gatherTime = 1f;

        // Whether the player is currently gathering this herb
        private bool _isGathering;
        private Coroutine _gatherCoroutine;
        private PlayerCharacter _currentPlayer;
        [SerializeField] private SpriteRenderer _spriteRenderer;

        public SpriteRenderer SpriteRenderer => _spriteRenderer;

        public string Name => "Herb";

        /// <summary>
        /// Called when the player clicks on this herb to start gathering.
        /// </summary>
        public void Interact(PlayerCharacter player)
        {
            Debug.Log($"Player {player.name} is gathering {name}...");
            // Prevent multiple gather attempts
            if (_isGathering) return;

            _currentPlayer = player;
            _gatherCoroutine = StartCoroutine(GatherRoutine(player));
        }
        
        public void OnHighlight()
        {
            if(SpriteRenderer)
                SpriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f);
        }

        public void OnHighlightEnd()
        {
            if(SpriteRenderer)
                SpriteRenderer.color = Color.white;
        }

    
        public void InteractEnd()
        {
            if (_gatherCoroutine != null)
            {
                StopCoroutine(_gatherCoroutine);
                _gatherCoroutine = null;
            }
            _isGathering = false;
            OnHighlightEnd();
        }

        /// <summary>
        /// Coroutine that tracks gather time and checks for interruptions.
        /// </summary>
        private IEnumerator GatherRoutine(PlayerCharacter player)
        {
            var gatherTime = _gatherTime;
            var elapsed = 0f;
            while (elapsed < gatherTime)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
            // Gathering complete
            Debug.Log("Herb gathered!");
            _isGathering = false;
            _gatherCoroutine = null;
            Drop();
            
        }

        /// <summary>
        /// Cancels an ongoing gather attempt.
        /// </summary>
        private void CancelGather()
        {
            if (_gatherCoroutine != null)
            {
                StopCoroutine(_gatherCoroutine);
                _gatherCoroutine = null;
            }
            _isGathering = false;
        }

        /// <summary>
        /// Called when the herb has been successfully gathered.
        /// Implementation is left empty for now.
        /// </summary>
        private void Drop()
        {
            GetComponent<DropItemComponent>()?.DropItems();
            CancelGather();
            Destroy(gameObject);
        }
    }
}