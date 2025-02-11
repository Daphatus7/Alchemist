using System;
using System.Collections.Generic;
using _Script.UserInterface;
using _Script.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

// For HexGrid, HexNode, etc.

namespace _Script.Map.WorldMap
{
    /// <summary>
    /// Responsible solely for the visual presentation of the map and forwarding user inputs to the controller.
    /// </summary>
    public class MapExplorerView : Singleton<MapExplorerView>, IUIHandler
    {
        [Header("Cameras")]
        [SerializeField] private Camera gameCamera;
        [SerializeField] private Camera mapCamera;

        [Header("Grid Settings")]
 
        [SerializeField] private GameObject hexPrefab;
        [SerializeField] private Grid mapGrid;
        [SerializeField] private GameObject mapCanvas;

        [Header("Node Sprites")]
        [SerializeField] private Sprite emptyNodeSprite;
        [SerializeField] private Sprite resourceNodeSprite;
        [SerializeField] private Sprite enemyNodeSprite;
        [SerializeField] private Sprite campfireNodeSprite;
        [SerializeField] private Sprite obstacleNodeSprite;
        [SerializeField] private Sprite bossNodeSprite;

        [Header("Map Interaction (Grid Scaling)")]
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 2f;
        [SerializeField] private float panSpeed = 1f;
        [SerializeField] private float zoomSpeed = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool debug = false;

        // Object pool for node visuals
        private ObjectPool<HexNodeDisplay> _hexNodePool;
        private readonly Dictionary<HexNode, HexNodeDisplay> _hexDisplayMap = new Dictionary<HexNode, HexNodeDisplay>();

        private MapController _controller; private MapController Controller
        {
            get
            {
                return _controller ??= MapController.Instance;
            }   
        }

        private bool _isMapOpen = false;
        private float _currentScale = 1f;

        // Variables for panning the map.
        private bool _isDragging = false;
        private Vector3 _lastMousePosition;

        private void Start()
        {
            InitializeNodePool();
            HideUI();

            // Start in the game view.
            if (mapCamera != null) mapCamera.enabled = false;
            if (gameCamera != null) gameCamera.enabled = true;
        }

        private void Update()
        {
            // Toggle the map display with the M key.
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMap();
            }

            if (_isMapOpen && mapCamera)
            {
                HandleMapInteraction();
            }
        }

        private void OnDisable()
        {
            if (Controller != null)
                Controller.SubscribeToNodeChange(OnControllerNodeChanged);  
        }

        private void OnControllerNodeChanged(HexNode node)
        {
            UpdateNodeVisual(node);
        }

        #region UI Display Methods

        public void ShowUI()
        {
            if (!mapCanvas) return;
            mapCanvas.SetActive(true);
            GenerateGridVisuals();
            if (Controller != null)
                Controller.SubscribeToNodeChange(OnControllerNodeChanged); 
            CenterMapCameraOnMap();
            if (gameCamera) gameCamera.enabled = false;
            if (mapCamera) mapCamera.enabled = true;

            _isMapOpen = true;
        }

        public void HideUI()
        {
            if (!mapCanvas) return;
            if(Controller)
                Controller.UnsubscribeFromNodeChange(OnControllerNodeChanged);
            mapCanvas.SetActive(false);

            if (gameCamera) gameCamera.enabled = true;
            if (mapCamera) mapCamera.enabled = false;

            _isMapOpen = false;
        }

        private void ToggleMap()
        {
            if (_isMapOpen)
                HideUI();
            else
                ShowUI();
        }

        private void CenterMapCameraOnMap()
        {
            if (mapCamera != null)
            {
                mapCamera.transform.position = new Vector3(0, 0, -10);
                _currentScale = 1f;
                mapGrid.transform.localScale = Vector3.one * _currentScale;
            }
        }

        private void HandleMapInteraction()
        {
            // Start dragging on left mouse button press.
            if (Input.GetMouseButtonDown(0))
            {
                _isDragging = true;
                _lastMousePosition = Input.mousePosition;
            }
            if (Input.GetMouseButtonUp(0))
            {
                _isDragging = false;
            }
            if (_isDragging)
            {
                Vector3 currentMousePos = Input.mousePosition;
                Vector2 delta = currentMousePos - _lastMousePosition;
                PanMap(delta);
                _lastMousePosition = currentMousePos;
            }
            // Allow middle mouse drag for panning.
            if (Input.GetMouseButton(2))
            {
                Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                PanMap(delta);
            }
            // Zoom the map with the scroll wheel.
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomMap(scroll);
            }
        }

        private void PanMap(Vector2 delta)
        {
            RectTransform rectTransform = mapCanvas.GetComponent<RectTransform>();
            Vector2 pos = rectTransform.anchoredPosition;
            pos += delta * panSpeed;
            rectTransform.anchoredPosition = pos;
        }

        private void ZoomMap(float amount)
        {
            _currentScale -= amount * zoomSpeed;
            _currentScale = Mathf.Clamp(_currentScale, minScale, maxScale);
            mapGrid.transform.localScale = Vector3.one * _currentScale;
        }

        #endregion

        #region Node Visuals

        private void InitializeNodePool()
        {
            var displayPrefab = hexPrefab.GetComponent<HexNodeDisplay>();
            _hexNodePool = new ObjectPool<HexNodeDisplay>(displayPrefab, mapGrid.transform, initialCapacity: 50);
        }

        /// <summary>
        /// Create a visual representation for each node from the controller’s grid.
        /// </summary>
        private void GenerateGridVisuals()
        {
            foreach (HexNode hexNode in Controller.HexGrid.GetAllVisibleHexNodes())
            {
                if (!_hexDisplayMap.ContainsKey(hexNode))
                {
                    Vector3 worldPos = GetWorldPosition(hexNode.Position);
                    HexNodeDisplay nodeDisplay = _hexNodePool.Get();
                    nodeDisplay.transform.position = worldPos;
                    _hexDisplayMap[hexNode] = ConfigureNodeDisplay(nodeDisplay, hexNode);
                }
            }
        }

        private HexNodeDisplay ConfigureNodeDisplay(HexNodeDisplay display, HexNode hexNode)
        {
            // Set the initial sprite based on the node type.
            display.SetImage(GetImageByNodeType(hexNode.NodeType));
            display.HexNode = hexNode;

            // Wire up UI events: when the player clicks on a node, notify the controller.
            display.OnNodeClicked+= OnClickedOnNode;
            display.OnNodeEnter+= OnHoverOnNode;
            display.OnNodeLeave+= OnLeaveNode;
            return display;
        }
    

        private void UpdateNodeVisual(HexNode node)
        {
            if (_hexDisplayMap.TryGetValue(node, out HexNodeDisplay nodeDisplay))
            {
                UpdateNodeVisual(node, nodeDisplay);
            }
            else
            {
                GenerateGridVisuals();
                if (_hexDisplayMap.TryGetValue(node, out HexNodeDisplay display))
                {
                    UpdateNodeVisual(node, display);
                }
            }
        }
        private void UpdateNodeVisual(HexNode node, HexNodeDisplay nodeDisplay)
        {
            switch (node.ExplorationState)
            {
                case NodeExplorationState.Revealed:
                    break;
                case NodeExplorationState.Explored:
                    nodeDisplay.SetNodeComplete();
                    break;
                case NodeExplorationState.Unrevealed:
                    Debug.Log("Node is unrevealed.");
                    break;
                case NodeExplorationState.Exploring:
                    nodeDisplay.SetNodeExploring();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Converts the logical node’s position to a world position (UI space).
        /// </summary>
        private Vector3 GetWorldPosition(Vector3Int cubePosition)
        {
            Vector2Int axial = CubeToAxial(cubePosition);
            Vector2Int offset = AxialToOddROffset(axial);
            Vector3Int cellPos = new Vector3Int(offset.x, offset.y, 0);
            return mapGrid.CellToWorld(cellPos);
        }

        private Vector2Int CubeToAxial(Vector3Int cube)
        {
            // For example, q = x, r = z.
            return new Vector2Int(cube.x, cube.z);
        }

        private Vector2Int AxialToOddROffset(Vector2Int axial)
        {
            int q = axial.x;
            int r = axial.y;
            int col = q + (r - (r & 1)) / 2;
            int row = r;
            return new Vector2Int(col, row);
        }

        private Sprite GetImageByNodeType(NodeType nodeType)
        {
            return nodeType switch
            {
                NodeType.Empty    => emptyNodeSprite,
                NodeType.Resource => resourceNodeSprite,
                NodeType.Enemy    => enemyNodeSprite,
                NodeType.Bonfire  => campfireNodeSprite,
                NodeType.Obstacle => obstacleNodeSprite,
                NodeType.Boss     => bossNodeSprite,
                _                 => emptyNodeSprite,
            };
        }

        private void OnClickedOnNode(HexNodeDisplay display)
        {
            // Forward the click to the controller.
            Controller.TryExploreNode(display.HexNode);
        }

        private void OnHoverOnNode(HexNodeDisplay display)
        {
            display.Highlight(true);
        }

        private void OnLeaveNode(HexNodeDisplay display)
        {
            display.Highlight(false);
        }

        #endregion

        #region Public Methods (e.g., Reset)

        [Button]
        public void ResetGrid()
        {
            // Return all node visuals to the pool.
            foreach (var kvp in _hexDisplayMap)
            {
                if (kvp.Value != null)
                    _hexNodePool.ReturnToPool(kvp.Value);
            }
            _hexDisplayMap.Clear();

            // Reset the controller (logic) and rebuild visuals.
            Controller.ResetGrid();
            GenerateGridVisuals();
            HideUI();
        }

        #endregion
    }
}
