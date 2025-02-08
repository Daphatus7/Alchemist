using System;
using _Script.Interactable;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Script.Character.ActionStrategy
{
    /// <summary>
    /// Returns the InteractionContext
    /// </summary>
    [DefaultExecutionOrder(300)]
    public class InteractionBase
    {
        private readonly LayerMask _interactableLayer = LayerMask.GetMask("Interactable");
        private readonly LayerMask _obstacleLayer = LayerMask.GetMask("Obstacle");
        
        private readonly float _maxInteractDistance;
        public InteractionBase(float maxInteractDistance)
        {
            _maxInteractDistance = maxInteractDistance;
        }
        
        public InteractionContext InteractableRaycast(Vector2 origin, Vector2 destination)
        {
            var direction = (destination - origin).normalized;
            var extent = Mathf.Min(Vector2.Distance(origin, destination), _maxInteractDistance);
            var hit = Physics2D.Raycast(origin, direction, extent, _interactableLayer);
            
            Debug.DrawLine(origin, origin + direction * extent, Color.red, 1f);
            return new InteractionContext(hit);
        }
        
        /// <summary>
        /// Gets the InteractionContext by checking what's under the mouse cursor.
        /// Interaction is only allowed if:
        ///  - The mouse isn't over UI.
        ///  - The destination is within 3f of the player.
        ///  - There are no obstacles between the player and the destination.
        /// </summary>
        public InteractionContext InteractableFromMouse(Vector2 playerPosition, Vector2 destination)
        {
            // Prevent interaction with world objects if the pointer is over a UI element.
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject())
            {
                return new InteractionContext(new RaycastHit2D());
            }
            
            if (Vector2.Distance(playerPosition, destination) > _maxInteractDistance)
            {
                return new InteractionContext(new RaycastHit2D());
            }
            
            // Check for obstacles between the player and the destination.
            Vector2 direction = (destination - playerPosition).normalized;
            float distance = Vector2.Distance(playerPosition, destination);
            RaycastHit2D obstacleHit = Physics2D.Raycast(playerPosition, direction, distance, _obstacleLayer);
            if (obstacleHit.collider != null)
            {
                // An obstacle is blocking the path.
                return new InteractionContext(new RaycastHit2D());
            }
            
            // Finally, cast a ray at the destination to detect interactable objects.
            RaycastHit2D hit = Physics2D.Raycast(destination, Vector2.zero, 0f, _interactableLayer);
            return new InteractionContext(hit);
        }
    }
    
    /// <summary>
    /// When interacting with an object, the context of the interaction is generated.
    /// </summary>
    public class InteractionContext
    {
        private readonly RaycastHit2D _hit;
        
        public InteractionContext(RaycastHit2D hit)
        {
            _hit = hit;
        }

        public string GetInteractableName() => _hit.collider ? _hit.collider.name : "No Interactable";

        public bool Interact(PlayerCharacter player)
        {
            if (!_hit.collider) return false;
            var interactable = _hit.collider.GetComponent<IInteractable>();
            if (interactable == null) return false;
            interactable.Interact(player);
            return true;
        }
        
        // public void StopInteract(GameObject player)
        // {
        //     if (!_hit.collider) return;
        //     var interactable = _hit.collider.GetComponent<IInteractable>();
        //     interactable?.InteractEnd();
        // }

        public bool Highlight(out IInteractable interactable)
        {
            interactable = null;
            if (!_hit.collider) return false;
            interactable = _hit.collider.GetComponent<IInteractable>();
            interactable?.OnHighlight();
            return interactable != null;
        }
        
        public void StopHighlight()
        {
            if (!_hit.collider) return;
            var interactable = _hit.collider.GetComponent<IInteractable>();
            interactable?.OnHighlightEnd();
        }
    }
}
