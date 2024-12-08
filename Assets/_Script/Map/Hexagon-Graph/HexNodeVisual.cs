using UnityEngine;
using UnityEngine.Events;

namespace _Script.Map.Hexagon_Graph
{
    public class HexNodeVisual : MonoBehaviour, INodeHandle
    {
        public HexNode HexNode;
        
        [SerializeField] private SpriteRenderer iconRenderer;
        [SerializeField] private SpriteRenderer highlightRenderer;
        
        public UnityEvent<INodeHandle> OnNodeClicked = new UnityEvent<INodeHandle>();
        public UnityEvent<INodeHandle> OnNodeEnter = new UnityEvent<INodeHandle>();
        public UnityEvent<INodeHandle> OnNodeLeave = new UnityEvent<INodeHandle>();
        
        private void Start()
        {
            highlightRenderer.enabled = false;
        }

        public void SetImage(Sprite sprite)
        {
            iconRenderer.sprite = sprite;
        }
        
        public void Highlight(bool isHighlighted)
        {
            highlightRenderer.enabled = isHighlighted;
        }

        
        private void OnMouseEnter()
        {
            OnNodeEnter?.Invoke(this);
        }

        private void OnMouseExit()
        {
            OnNodeLeave?.Invoke(this);
        }

        private void OnMouseDown()
        {
            OnNodeClicked?.Invoke(this);
        }
        
        public Vector3Int GetPosition()
        {
            return HexNode.Position;
        }
    }
}