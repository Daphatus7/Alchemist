// /**
//  *
//  *   Author: daphatus
//  *   File: ${File.Name}
//  *   Date: $[InvalidReference]
//  */

using UnityEngine;

namespace _Script.Interactable
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class AutoTrigger : Interactable
    {
        protected Collider2D Collider; 
        
        protected override void Awake()
        {
            base.Awake();
            Collider = GetComponent<Collider2D>();
            Collider.isTrigger = true;
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                Interact();
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                InteractCanceled();
            }
        }
    }
}