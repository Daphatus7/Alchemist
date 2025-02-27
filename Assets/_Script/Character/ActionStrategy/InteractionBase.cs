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
            throw new NotImplementedException();
            // var direction = (destination - origin).normalized;
            // var extent = Mathf.Min(Vector2.Distance(origin, destination), _maxInteractDistance);
            // var hit = Physics2D.Raycast(origin, direction, extent, _interactableLayer);
            //
            // Debug.DrawLine(origin, origin + direction * extent, Color.red, 1f);
            // return new InteractionContext(hit);
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
                return null;
            }
            
            if (Vector2.Distance(playerPosition, destination) > _maxInteractDistance)
            {
                return null;
            }
            
            // Check for obstacles between the player and the destination.
            Vector2 direction = (destination - playerPosition).normalized;
            float distance = Vector2.Distance(playerPosition, destination);
            RaycastHit2D obstacleHit = Physics2D.Raycast(playerPosition, direction, distance, _obstacleLayer);
            if (obstacleHit.collider)
            {
                // An obstacle is blocking the path.
                return null;
            }
            
            // Finally, cast a ray at the destination to detect interactable objects.
            RaycastHit2D hit = Physics2D.Raycast(destination, Vector2.zero, 0f, _interactableLayer);
            if (!hit.collider)
            {
                return null;
            }
            IInteractable i = hit.collider?.GetComponent<IInteractable>();
            return i == null ? null : new InteractionContext(i);
        }
    }
    
    /// <summary>
    /// When interacting with an object, the context of the interaction is generated.
    /// </summary>
    public class InteractionContext
    {
        private readonly IInteractable _hit;
        public InteractionContext(IInteractable hit)
        {
            _hit = hit;
        }
        
        public bool Equals(InteractionContext other)
        {
            return _hit == other._hit;
        }

        public string GetInteractableName() => _hit?.Name;

        public bool Interact(PlayerCharacter player)
        {
            if (_hit == null) return false;
            _hit.Interact(player);
            return true;
        }
        
        /// <summary>
        /// Called when the interaction should be forcibly stopped (e.g., mouse button released).
        /// </summary>
        public void StopInteract(PlayerCharacter player)
        {
            _hit?.InteractEnd();
        }

        public bool Highlight()
        {
            if (_hit == null) return false;
            _hit.OnHighlight();
            return true;
        }

        public void StopHighlight()
        {
            _hit?.OnHighlightEnd();
        }
    }
}
