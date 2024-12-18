using System.Collections.Generic;
using _Script.Managers;
using _Script.UserInterface;
using _Script.Utilities;
using UnityEngine;

namespace _Script.Map.WorldMap
{
    public class MapExplorerUI : Singleton<MapExplorerUI>, IUIHandler
    {
        [Header("Grid Settings")]
        [SerializeField] private float hexSize = 0.5f;
        [SerializeField] private int gridRadius = 10;
        [SerializeField] private int gridVisibility = 2;
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

        [SerializeField] private bool debug = true;
        
        private HexGrid _hexGrid;
        private HexNode _startHex;
        private ObjectPool<HexNodeDisplay> _hexNodePool;
        
        private readonly Dictionary<HexNode, HexNodeDisplay> _hexDisplayMap = new Dictionary<HexNode, HexNodeDisplay>();

        private void Start()
        {
            InitializeNodePool();
            InitializeHexGrid();
            SetupInitialMapState();
            HideUI();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                ToggleUI();
            }
        }
        
        private void ToggleUI()
        {
            if (mapCanvas != null)
            {
                mapCanvas.gameObject.SetActive(!mapCanvas.gameObject.activeSelf);
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
            }
        }

        public void ShowUI()
        {
            if (mapCanvas != null)
            {
                mapCanvas.gameObject.SetActive(true);
            }
        }

        private void GenerateGridVisuals()
        {
            foreach (HexNode hexNode in _hexGrid.GetAllVisibleHexNodes())
            {
                if (!_hexDisplayMap.ContainsKey(hexNode))
                {
                    var worldPosition = GetWorldPosition(hexNode.Position);
                    
                    //get visual ref
                    var nodeDisplay = _hexNodePool.Get();
                    
                    //set position
                    nodeDisplay.transform.position = worldPosition;
                    
                    //set node button
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
            // Cube to axial: q = x, r = z
            return new Vector2Int(cube.x, cube.z);
        }

        private Vector2Int AxialToOddROffset(Vector2Int axial)
        {
            // Convert axial coords to odd-r offset coords.
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
                Debug.Log("Node is not adjacent or not in the correct state to explore.");
            }
        }

        private void ExploreNode(INodeHandle handle, HexNode node)
        {
            /*Debug section*/
            if (debug)
            {
                _hexGrid.MovePlayer(node);
                node.SetExplorationState(NodeExplorationState.Exploring);

                MarkCurrentNodeAsExplored();
                
                return;
            }
            return;
            /**/
            HideUI();
            _hexGrid.MovePlayer(node);
            node.SetExplorationState(NodeExplorationState.Exploring);
            handle.SetNodeExploring();
            GameManager.Instance.LoadSelectedScene(node.NodeData);
        }

        public void MarkCurrentNodeAsExplored()
        {
            //get player position
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

        public void ResetHexMap()
        {
            // Return all currently active hex displays to the pool
            foreach (var kvp in _hexDisplayMap)
            {
                if (kvp.Value != null && kvp.Value.gameObject != null)
                {
                    _hexNodePool.ReturnToPool(kvp.Value);
                }
            }
            _hexDisplayMap.Clear();

            // Re-initialize and refresh the map
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