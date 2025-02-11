using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Script.Map.WorldMap
{
    [System.Serializable] 
    public class HexNodeDisplay : MonoBehaviour, INodeHandle, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
    {
        private HexNode _hexNode;
        public HexNode HexNode
        {
            get => _hexNode;
            set
            {
                _hexNode = value;

                // Update the text to show node level + interpolated value
                _nodeText.text = $"L{_hexNode.NodeLevel}\n{_hexNode.InterpolatedValue:F2}";

                // Debug: set image color based on the InterpolatedValue
                if (_nodeImage != null)
                {
                    float val = Mathf.Clamp(_hexNode.InterpolatedValue, 0f, 10f);
                    float normalizedVal = val / 10f;

                    // Define color stops
                    var colors = new Color[] {
                        Color.blue,    // 0.0
                        Color.cyan,    // 0.25
                        Color.green,   // 0.5
                        Color.yellow,  // 0.75
                        Color.red      // 1.0
                    };

                    float step = 1f / (colors.Length - 1);
                    int index = Mathf.FloorToInt(normalizedVal / step);
                    int maxIndex = colors.Length - 2;
                    index = Mathf.Clamp(index, 0, maxIndex);

                    float t = (normalizedVal - (index * step)) / step;
                    Color debugColor = Color.Lerp(colors[index], colors[index + 1], t);

                    _nodeImage.color = debugColor;

                    // Remember this as the "original" or "base" color
                    _originalColor = debugColor;
                }
            }
        }

        private Image _nodeImage;
        [SerializeField] private TextMeshProUGUI _nodeText;
        [SerializeField] private Sprite explorationSprite;

        [Range(0f, 1f)]
        [SerializeField] private float alphaHitTestThreshold = 0.1f;

        public event Action<HexNodeDisplay> OnNodeClicked = new Action<HexNodeDisplay>(node => { });
        public event Action<HexNodeDisplay> OnNodeEnter = new Action<HexNodeDisplay>(node => { });
        public event Action<HexNodeDisplay> OnNodeLeave = new Action<HexNodeDisplay>(node => { });

        // Used to store the color before highlighting
        private Color _originalColor;

        private void Awake()
        {
            if (_nodeImage == null)
            {
                _nodeImage = GetComponent<Image>();
                _nodeImage.alphaHitTestMinimumThreshold = alphaHitTestThreshold;
            }
        }

        private bool _isHighlighted = false;

        public void SetImage(Sprite sprite)
        {
            if (_nodeImage != null)
            {
                _nodeImage.sprite = sprite;
            }
        }

        public void Highlight(bool state)
        {
            if (_hexNode.ExplorationState == NodeExplorationState.Explored)
            {
                return;
            }

            _isHighlighted = state;
            if (_nodeImage != null)
            {
                _nodeImage.color = state ? Color.yellow : _originalColor;
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
            // Mark visually as completed if needed
            // if (_nodeImage != null) _nodeImage.color = Color.blue;
            MarkExploredVisual();
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
            return _hexNode.Position;
        }

        // IPointerClickHandler
        public void OnPointerClick(PointerEventData eventData)
        {
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

        public void SetNodeRevealed()
        {
   
        }
    }
}
