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

        public void Interact()
        {
            if (_hit.collider != null)
            {
                var interactable = _hit.collider.GetComponent<Interactable.Interactable>();
                if (interactable != null)
                {
                    interactable.Interact();
                }
            }
        }
        
        public void StopInteract()
        {
            if (_hit.collider != null)
            {
                var interactable = _hit.collider.GetComponent<Interactable.Interactable>();
                if (interactable != null)
                {
                    interactable.InteractEnd();
                }
            }
        }
        
    }
}