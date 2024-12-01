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

        protected virtual void Awake()
        {
            //make sure all interactable objects are on the interactable layer
            gameObject.layer = LayerMask.NameToLayer("Interactable"); 
        }
        
        /// <summary>
        /// Checked before the player is allowed to interact with the object
        /// </summary>
        /// <returns></returns>
        protected abstract bool CanInteract();
        
        /// <summary>
        /// Called when the player interacts with the object
        /// </summary>
        protected abstract void OnInteract();

        
        /// <summary>
        /// ramification of the player canceling the interaction
        /// </summary>
        protected abstract void OnInteractCanceled();

        
        /// <summary>
        /// Called when the player has completed the interaction
        /// </summary>
        protected abstract void OnInteractCompleted();


        #region Trigger

        
        /// <summary>
        /// Trigger Start
        /// </summary>
        public void Interact()
        {
            if(CanInteract())
            {
                _isInteracting = true;
                OnInteract();
            }
        }
        
        /// <summary>
        /// Trigger Cancel
        /// </summary>
        public void InteractCanceled()
        {
            _isInteracting = false;
            OnInteractCanceled();
        }

        #endregion

    }
}