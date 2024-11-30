using System;
using UnityEngine;

namespace _Script.Interactable
{
    public abstract class Interactable : MonoBehaviour
    {
        /**
         * if the object is interactable and the player is actively interacting with it
         */
        private bool _isInteracting = false; public bool IsInteracting => _isInteracting;
        
        private void Awake()
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable"); 
        }
        
        /*
         * Time tracking for interaction
         */
        private float _timeElapsed = 0f;
        /*
         * Total time required to interact with the object
         */
        [SerializeField] private float _timeToComplete = 0.5f;
        
        private void Update()
        {
            if (_isInteracting)
            {
                OnInteract();
            }
        }
        
        protected abstract bool CanInteract();

        private void OnInteract()
        {
            _timeElapsed += Time.deltaTime;
            Debug.Log("Interacting with " + _timeElapsed);
            if (_timeElapsed >= _timeToComplete)
            {
                Debug.Log("Interact completed");
                OnInteractCompleted();
            }
        }

        private void OnInteractCanceled()
        {
        }

        protected abstract void OnInteractCompleted();


        #region Trigger

        
        public void OnStartInteract()
        {
            //can interact
            if (!CanInteract()) return;
            _isInteracting = true;
        }
        
        public void OnEndInteract()
        {
            _isInteracting = false;
            OnInteractCanceled();
        }
        
        #endregion

    }
}