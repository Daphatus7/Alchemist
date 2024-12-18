using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Script.Map.WorldMap
{
    [System.Serializable] 
    public class NodeEvent : UnityEvent<INodeHandle> { }

    public class HexNodeDisplay : MonoBehaviour, INodeHandle, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public HexNode HexNode;
        private Image _nodeImage;
        [SerializeField] private Sprite explorationSprite;

        [Range(0f, 1f)]
        [SerializeField] private float alphaHitTestThreshold = 0.1f;
        
        public NodeEvent OnNodeClicked = new NodeEvent();
        public NodeEvent OnNodeEnter = new NodeEvent();
        public NodeEvent OnNodeLeave = new NodeEvent();

        private void Awake()
        {
            if (_nodeImage == null)
            {
                _nodeImage = GetComponent<Image>();
                _nodeImage.alphaHitTestMinimumThreshold = alphaHitTestThreshold;
            }
        }
        
        private bool isHighlighted = false;

        public void SetImage(Sprite sprite)
        {
            if (_nodeImage != null)
            {
                _nodeImage.sprite = sprite;
            }
        }

        public void Highlight(bool state)
        {
            if (HexNode.ExplorationState == NodeExplorationState.Explored)
            {
                return;
            }

            isHighlighted = state;
            if (_nodeImage != null)
            {
                _nodeImage.color = state ? Color.yellow : Color.white;
            }
        }

        public void MarkExploredVisual()
        {
            if (_nodeImage != null)
            {
                _nodeImage.sprite = explorationSprite;
            }
        }

        public void SetNodeComplete()
        {
            // Mark visually as completed if needed, e.g.:
            // if (_nodeImage != null) _nodeImage.color = Color.blue;
        }

        public void SetNodeExploring()
        {
            if (_nodeImage != null)
            {
                _nodeImage.sprite = explorationSprite;
            }
        }

        public Vector3Int GetPosition()
        {
            return HexNode.Position;
        }

        // IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
            Debug.Log("Clicked on node: " + HexNode.Position);
            OnNodeClicked.Invoke(this);
        }

        // IPointerEnterHandler
        public void OnPointerEnter(PointerEventData eventData)
        {
            OnNodeEnter.Invoke(this);
        }

        // IPointerExitHandler
        public void OnPointerExit(PointerEventData eventData)
        {
            OnNodeLeave.Invoke(this);
        }
    }
}