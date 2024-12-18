using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace _Script.Map.WorldMap
{
    [System.Serializable] public class NodeEvent : UnityEvent<INodeHandle> { }

    public class HexNodeDisplay : MonoBehaviour, INodeHandle
    {
        public HexNode HexNode;
        [SerializeField] private SpriteRenderer nodeImage;
        [SerializeField] private Sprite explorationSprite;

        public NodeEvent OnNodeClicked = new NodeEvent();
        public NodeEvent OnNodeEnter = new NodeEvent();
        public NodeEvent OnNodeLeave = new NodeEvent();

        private bool isHighlighted = false;

        public void SetImage(Sprite sprite)
        {
            nodeImage.sprite = sprite;
        }

        public void Highlight(bool state)
        {
            //if explored, do not highlight
            if (HexNode.ExplorationState == NodeExplorationState.Explored)
            {
                return;
            }
            
            isHighlighted = state;
            // For example, we can change the color or outline when highlighted:
            nodeImage.color = state ? Color.yellow : Color.white;
        }
        
        public void MarkExploredVisual()
        {
            // Mark visually as explored (e.g., change color to green)
            nodeImage.sprite = explorationSprite;
        }

        public void SetNodeComplete()
        {
            // Mark visually as completed (e.g., change color to blue)
        }

        public void SetNodeExploring()
        {
            // Mark visually as exploring (e.g., change color to cyan)
            nodeImage.color = Color.cyan;
        }

        public Vector3Int GetPosition()
        {
            return HexNode.Position;
        }

        // These can be triggered by Unity's EventSystem 
        // (e.g., OnPointerClick, OnPointerEnter, OnPointerExit)
        private void OnMouseDown()
        {
            OnNodeClicked.Invoke(this);
        }

        private void OnMouseEnter()
        {
            OnNodeEnter.Invoke(this);
        }

        private void OnMouseExit()
        {
            OnNodeLeave.Invoke(this);
        }
        
    }
}