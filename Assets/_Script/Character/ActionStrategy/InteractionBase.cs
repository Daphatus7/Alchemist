using _Script.Interactable;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    /// <summary>
    /// Base Interaction Strategy that always exist regardless of Action Strategy
    /// </summary>
    [DefaultExecutionOrder(300)]
    public class InteractionBase
    {
        private readonly LayerMask _interactableLayer = LayerMask.GetMask("Interactable");
        private RaycastHit2D _hit;
        private RaycastHit2D _circleHit;
        [SerializeField] private float _maxDistance = 2f;
        [SerializeField] private float _circleRadius = 1f;
        

        public InteractionContext InteractableRaycast(Vector2 origin, Vector2 destination)
        {
            var direction = (destination - origin).normalized;
            var extent = Mathf.Min(Vector2.Distance(origin, destination), _maxDistance);
            _hit = Physics2D.Raycast(origin, direction, extent, _interactableLayer);
            
            Debug.DrawLine(origin, origin + direction * extent, Color.red, 1f);
            return new InteractionContext(_hit);
        }
    }

    public class InteractionContext
    {
        private readonly RaycastHit2D _hit;
        
        public InteractionContext(RaycastHit2D hit)
        {
            _hit = hit;
        }

        public string GetInteractableName()
        {
            return _hit.collider != null ? _hit.collider.name : "No Interactable";
        }
        public bool Interact(GameObject player)
        {
            if (!_hit.collider) return false;
            var interactable = _hit.collider.GetComponent<IInteractable>();
            if (interactable == null)
            { 
                return false;
            }
            else
            {
                interactable.Interact(player);
                return true;
            }
        }
        
        public void StopInteract(GameObject player)
        {
            if (!_hit.collider) return;
            var interactable = _hit.collider.GetComponent<IInteractable>();
            interactable?.InteractEnd(player);
        }
        
        
        /// <summary>
        /// Highlight the object by calling the interface method
        /// </summary>
        /// <param name="interactable"></param>
        /// <returns>has highlighted objects</returns>
        public bool Highlight(out IInteractable interactable)
        {
            //if there is no collider hit
            if (!_hit.collider)
            {
                interactable = null;
                return false;
            }
            interactable = _hit.collider.GetComponent<IInteractable>();
            
            //if there is no interactable object
            if (interactable != null)
            {
                interactable.OnHighlight();
                return true;            
            }
            return false;
        }
        
        public void StopHighlight()
        {
            if (!_hit.collider) return;
            var interactable = _hit.collider.GetComponent<IInteractable>();
            interactable?.OnHighlightEnd();
        }
        
    }
}