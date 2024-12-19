using System.Collections.Generic;
using _Script.Managers;
using _Script.UserInterface;
using _Script.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map.WorldMap
{
    public class MapExplorerUI : Singleton<MapExplorerUI>, IUIHandler
    {
        [Header("Cameras")]
        [SerializeField] private Camera gameCamera;   // The main game camera for normal gameplay
        [SerializeField] private Camera mapCamera;    // The camera used when viewing the map

        [Header("Grid Settings")]
        [SerializeField] private float hexSize = 0.5f;
        [SerializeField] private int gridRadius = 20;
        [SerializeField] private int gridVisibility = 2;
        [SerializeField] private GameObject hexPrefab;
        [SerializeField] private Grid mapGrid;        // UI Grid that holds hex nodes
        [SerializeField] private GameObject mapCanvas;// UI Canvas for the map UI

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
        [SerializeField] private float zoomSpeed = 0.2f; // Adjust as needed

        [Header("Debug")]
        [SerializeField] private bool debug = false; // If true, exploring a node just marks it as explored without returning to game view

        private HexGrid _hexGrid;
        private HexNode _startHex;
        private ObjectPool<HexNodeDisplay> _hexNodePool;
        private readonly Dictionary<HexNode, HexNodeDisplay> _hexDisplayMap = new Dictionary<HexNode, HexNodeDisplay>();

        private bool isMapOpen = false;
        private float currentScale = 1f; // Current scale factor of the mapGrid

        // Fields for dragging (panning) with the left mouse button
        private bool isDragging = false;
        private Vector3 lastMousePosition;
        


        private void Start()
        {
            InitializeNodePool();
            InitializeHexGrid();
            SetupInitialMapState();
            HideUI();

            // Initially show only the game camera
            if (mapCamera != null) mapCamera.enabled = false;
            if (gameCamera != null) gameCamera.enabled = true;
        }

        private void Update()
        {
            // Press M to toggle the map view
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleMap();
            }

            // If the map is open, handle panning and zooming by scaling the grid
            if (isMapOpen && mapCamera)
            {
                HandleMapInteraction();
            }
        }

        private void OnDisable()
        {
            if (_hexGrid != null)
                _hexGrid.OnNodeChanged -= HandleNodeChanged;
        }

        private void HandleNodeChanged(HexNode node)
        {
            UpdateNodeVisual(node.Position);
        }
        

        /// <summary>
        /// Toggles between the main game view and the map view.
        /// </summary>
        private void ToggleMap()
        {
            isMapOpen = !isMapOpen;

            if (isMapOpen)
            {
                // Switch to the map view
                if (gameCamera) gameCamera.enabled = false;
                if (mapCamera) mapCamera.enabled = true;

                ShowUI();
                CenterMapCameraOnMap();
            }
            else
            {
                // Switch back to the game view
                if (mapCamera != null) mapCamera.enabled = false;
                if (gameCamera != null) gameCamera.enabled = true;

                HideUI();
            }
        }

        /// <summary>
        /// Handles map camera interaction:
        /// - Left mouse drag to "drag and drop" the mapCanvas in a 1:1 pixel ratio
        /// - Middle mouse drag also pans (optional redundancy, can remove if unnecessary)
        /// - Mouse scroll wheel scales the grid (simulate zoom)
        /// </summary>
        private void HandleMapInteraction()
        {
            // Start dragging if left mouse button is pressed
            if (Input.GetMouseButtonDown(0))
            {
                isDragging = true;
                lastMousePosition = Input.mousePosition;
            }

            // End dragging if left mouse button is released
            if (Input.GetMouseButtonUp(0))
            {
                isDragging = false;
            }

            // If dragging with left mouse button
            if (isDragging)
            {
                Vector3 currentMousePos = Input.mousePosition;
                Vector2 mouseDelta = currentMousePos - lastMousePosition;
                PanMap(mouseDelta);
                lastMousePosition = currentMousePos;
            }

            // Middle mouse drag = pan as well (optional)
            if (Input.GetMouseButton(2))
            {
                Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
                PanMap(mouseDelta);
            }

            // Scroll to "zoom" by scaling the grid
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.01f)
            {
                ZoomMap(scroll);
            }
        }

        private void PanMap(Vector2 delta)
        {
            // Get the RectTransform of the mapCanvas
            RectTransform rectTransform = mapCanvas.GetComponent<RectTransform>();

            // Directly add delta to anchoredPosition, resulting in a 1:1 movement
            Vector2 pos = rectTransform.anchoredPosition;
            pos += delta; // no panSpeed, no scaling, direct movement in pixels
            rectTransform.anchoredPosition = pos;
        }

        /// <summary>
        /// Zoom the map by scaling the mapGrid's transform instead of changing camera size.
        /// </summary>
        private void ZoomMap(float amount)
        {
            currentScale -= amount * zoomSpeed;
            currentScale = Mathf.Clamp(currentScale, minScale, maxScale);
            mapGrid.transform.localScale = Vector3.one * currentScale;
        }

        /// <summary>
        /// Centers the map camera on the map's starting area.
        /// Reset scale and position camera at a fixed point.
        /// </summary>
        private void CenterMapCameraOnMap()
        {
            if (mapCamera != null)
            {
                mapCamera.transform.position = new Vector3(0, 0, -10);
                currentScale = 1f;
                mapGrid.transform.localScale = Vector3.one * currentScale;
            }
        }

        private void InitializeNodePool()
        {
            var displayPrefab = hexPrefab.GetComponent<HexNodeDisplay>();
            _hexNodePool = new ObjectPool<HexNodeDisplay>(displayPrefab, mapGrid.transform, initialCapacity: 50);
        }

        private void InitializeHexGrid()
        {
            _hexGrid = new HexGrid(gridRadius, new GridConfiguration(hexSize));
        }

        private void SetupInitialMapState()
        {
            var spawnPoint = _hexGrid.GenerateSpawnPoint();
            _hexGrid.RevealHexNodeInRange(spawnPoint.x, spawnPoint.y, spawnPoint.z, gridVisibility);

            _startHex = _hexGrid.GetHexNode(spawnPoint.x, spawnPoint.y, spawnPoint.z);
            GenerateGridVisuals();

            if (_hexDisplayMap.TryGetValue(_startHex, out var startDisplay))
            {
                startDisplay.Highlight(true);
            }
        }

        public void HideUI()
        {
            if (mapCanvas != null)
            {
                mapCanvas.gameObject.SetActive(false);
                _hexGrid.OnNodeChanged -= HandleNodeChanged;
            }
        }

        public void ShowUI()
        {
            if (mapCanvas != null)
            {
                mapCanvas.gameObject.SetActive(true);
                _hexGrid.OnNodeChanged += HandleNodeChanged;
            }
        }

        private void GenerateGridVisuals()
        {
            foreach (HexNode hexNode in _hexGrid.GetAllHexNodes())
            {
                if (!_hexDisplayMap.ContainsKey(hexNode))
                {
                    var worldPosition = GetWorldPosition(hexNode.Position);
                    var nodeDisplay = _hexNodePool.Get();
                    nodeDisplay.transform.position = worldPosition;
                    _hexDisplayMap[hexNode] = ConfigureNodeDisplay(nodeDisplay, hexNode);
                }
            }
        }
        
        

        private Vector3 GetWorldPosition(Vector3Int cubePosition)
        {
            var axial = CubeToAxial(cubePosition);
            var offset = AxialToOddROffset(axial);
            var cellPosition = new Vector3Int(offset.x, offset.y, 0);
            return mapGrid.CellToWorld(cellPosition);
        }

        private Vector2Int CubeToAxial(Vector3Int cube)
        {
            // Convert cube coords to axial: q = x, r = z
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

        private void OnClickedOnNode(INodeHandle handle)
        {
            var pos = handle.GetPosition();
            var node = _hexGrid.GetHexNode(pos.x, pos.y, pos.z);

            if (node == null) return;

            if (_hexGrid.IsAdjacentToPlayer(node) && node.ExplorationState == NodeExplorationState.Revealed)
            {
                ExploreNode(handle, node);
            }
            else if (node.ExplorationState == NodeExplorationState.Explored)
            {
                Debug.Log("Node already explored. No further exploration possible.");
            }
            else
            {
                Debug.Log("This node cannot be explored from here.");
            }
        }

        private void ExploreNode(INodeHandle handle, HexNode node)
        {
            // If debug is enabled, just mark node as explored and stay in map mode
            if (debug)
            {
                _hexGrid.MovePlayer(node);
                node.SetExplorationState(NodeExplorationState.Exploring);
                MarkCurrentNodeAsExplored();
                return;
            }

            // Normal: switch back to game view after exploring
            HideUI();
            if (mapCamera != null) mapCamera.enabled = false;
            if (gameCamera != null) gameCamera.enabled = true;

            _hexGrid.MovePlayer(node);
            node.SetExplorationState(NodeExplorationState.Exploring);

            MarkCurrentNodeAsExplored();

            isMapOpen = false;
        }

        public void MarkCurrentNodeAsExplored()
        {
            var playerPos = _hexGrid.PlayerPosition;
            var node = _hexGrid.GetHexNode(playerPos.x, playerPos.y, playerPos.z);

            if (node.ExplorationState == NodeExplorationState.Exploring)
            {
                node.SetExplorationState(NodeExplorationState.Explored);

                _hexDisplayMap[node].SetNodeComplete();
                _hexGrid.RevealSurroundingNodes(node, gridVisibility);
                GenerateGridVisuals();
            }
        }

        private void OnHoverOnNode(INodeHandle handle)
        {
            handle.Highlight(true);
        }

        private void OnLeaveNode(INodeHandle handle)
        {
            handle.Highlight(false);
        }

        private Sprite GetImageByNodeType(NodeType nodeType) => nodeType switch
        {
            NodeType.Empty    => emptyNodeSprite,
            NodeType.Resource => resourceNodeSprite,
            NodeType.Enemy    => enemyNodeSprite,
            NodeType.Bonfire  => campfireNodeSprite,
            NodeType.Obstacle => obstacleNodeSprite,
            NodeType.Boss     => bossNodeSprite,
            _                 => emptyNodeSprite
        };

        [Button]
        public void ResetGrid()
        {
            _hexGrid.DebugResetGrid();
        }

        private HexNodeDisplay ConfigureNodeDisplay(HexNodeDisplay display, HexNode hexNode)
        {
            if (hexNode.ExplorationState == NodeExplorationState.Explored)
            {
                display.SetImage(obstacleNodeSprite);
                return display;
            }

            display.SetImage(GetImageByNodeType(hexNode.NodeType));
            display.HexNode = hexNode;
            display.OnNodeClicked.AddListener(OnClickedOnNode);
            display.OnNodeEnter.AddListener(OnHoverOnNode);
            display.OnNodeLeave.AddListener(OnLeaveNode);

            return display;
        }
        
        private void UpdateNodeVisual(Vector3Int nodePos)
        {
            // 查找对应的HexNode
            var node = _hexGrid.GetHexNode(nodePos.x, nodePos.y, nodePos.z);
            if (node == null) return;

            // 检查该节点对应的显示对象
            if (_hexDisplayMap.TryGetValue(node, out var nodeDisplay))
            {
                // 更新节点的Sprite等UI信息
                nodeDisplay.SetImage(GetImageByNodeType(node.NodeType));
            
                // 如果需要根据节点的ExplorationState变化，对应执行一些UI操作
                if (node.ExplorationState == NodeExplorationState.Explored)
                {
                    nodeDisplay.SetNodeComplete();
                }
                else
                {
                    nodeDisplay.Highlight(node.ExplorationState == NodeExplorationState.Exploring);
                }
            }
            else
            {
                // 若当前节点还没有对应的显示对象（某些情况下可能出现）
                // 则重新生成或调用GenerateGridVisuals()保证UI同步。
                // 通常生成一次后就有对应映射，不需再次生成。
            }
        }
        

        public void ResetHexMap()
        {
            foreach (var kvp in _hexDisplayMap)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    _hexNodePool.ReturnToPool(kvp.Value);
                }
            }
            _hexDisplayMap.Clear();

            InitializeHexGrid();
            var spawnPoint = _hexGrid.GenerateSpawnPoint();
            _hexGrid.RevealHexNodeInRange(spawnPoint.x, spawnPoint.y, spawnPoint.z, gridVisibility);

            _startHex = _hexGrid.GetHexNode(spawnPoint.x, spawnPoint.y, spawnPoint.z);
            GenerateGridVisuals();

            if (_hexDisplayMap.TryGetValue(_startHex, out var startDisplay))
            {
                startDisplay.Highlight(true);
            }

            HideUI();
        }
    }
}