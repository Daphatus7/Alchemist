using System;
using System.Collections.Generic;
using UnityEngine;
using _Script.Managers;

namespace _Script.Map.Hexagon_Graph
{
    public class MapExplorerUI : Singleton<MapExplorerUI>
    {
        [Header("Grid Settings")]
        public float hexSize = 0.5f;
        public int gridRadius = 10;
        public int gridVisibility = 2;
        public GameObject hexPrefab;
        public Grid mapGrid;

        private HexGrid _hexGrid;
        private Dictionary<HexNode, HexNodeDisplay> hexGameObjectMap = new Dictionary<HexNode, HexNodeDisplay>();

        [SerializeField] private Sprite emptyNodeSprite;
        [SerializeField] private Sprite resourceNodeSprite;
        [SerializeField] private Sprite enemyNodeSprite;
        [SerializeField] private Sprite campfireNodeSprite;
        [SerializeField] private Sprite obstacleNodeSprite;
        [SerializeField] private Sprite bossNodeSprite;

        private HexNode startHex;

        private void Start()
        {
            // 1. Generate the grid
            _hexGrid = new HexGrid(gridRadius, hexSize, new GridConfiguration(hexSize));

            // 2. Generate the spawn point
            var spawnPoint = _hexGrid.GenerateSpawnPoint();

            // 3. Reveal the grid around the spawn point
            _hexGrid.RevealHexNodeInRange(spawnPoint.x, spawnPoint.y, spawnPoint.z, gridVisibility);

            // 4. Get the start hex
            startHex = _hexGrid.GetHexNode(spawnPoint.x, spawnPoint.y, spawnPoint.z);

            // 5. Generate visual representations
            GenerateGridVisuals();

            // 6. Highlight the player's location
            if (hexGameObjectMap.ContainsKey(startHex))
            {
                hexGameObjectMap[startHex].Highlight(true);
            }
            
            //hide the grid initially
            HideGrid();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M))
            {
                HideGrid();
            }
        }

        public void HideGrid()
        {
            mapGrid.gameObject.SetActive(false);
        }
        
        public void ShowGrid()
        {
            mapGrid.gameObject.SetActive(true);
        }

        private void GenerateGridVisuals()
        {
            foreach (HexNode hexNode in _hexGrid.GetAllVisibleHexNodes())
            {
                if (!hexGameObjectMap.ContainsKey(hexNode))
                {
                    // Convert cube coords (x,y,z) to offset coords (col,row)
                    Vector2Int axial = CubeToAxial(hexNode.Position);
                    Vector2Int offset = AxialToOddROffset(axial);
                    
                    // mapGrid expects a Vector3Int for CellToWorld, z can be 0 as we're using a 2D layout
                    Vector3Int cellPosition = new Vector3Int(offset.x, offset.y, 0);
                    
                    // Use CellToWorld to get the world position from the hex grid
                    Vector3 worldPosition = mapGrid.CellToWorld(cellPosition);
                    
                    GameObject newNode = Instantiate(hexPrefab, worldPosition, Quaternion.identity, mapGrid.transform);
                    hexGameObjectMap.Add(hexNode, GenerateNodeVisual(newNode, hexNode));
                }
            }
        }

        // Convert cube to axial
        private Vector2Int CubeToAxial(Vector3Int cube)
        {
            int q = cube.x;
            int r = cube.z; 
            return new Vector2Int(q, r);
        }

        // Convert axial to offset (odd-r) coordinates
        // Adjust this if your grid orientation or layout differs
        private Vector2Int AxialToOddROffset(Vector2Int axial)
        {
            int q = axial.x;
            int r = axial.y;
            int col = q + (r - (r & 1)) / 2;
            int row = r;
            return new Vector2Int(col, row);
        }

        // This method is called when clicking on a node
        private void OnClickedOnNode(INodeHandle handle)
        {
            var node = _hexGrid.GetHexNode(handle.GetPosition().x, handle.GetPosition().y, handle.GetPosition().z);

            if (node == null) return;

            if (_hexGrid.IsAdjacentToPlayer(node) && node.ExplorationState == NodeExplorationState.Revealed)
            {
                //This is a valid node to explore
                HideGrid();
                
                _hexGrid.MovePlayer(node);
                node.SetExplorationState(NodeExplorationState.Exploring);
                handle.SetNodeExploring();
                // Typically load a scene:
                GameManager.Instance.LoadSelectedScene(node.MapNode);
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

        public void FinishExploringNode(HexNode node)
        {
            if (node.ExplorationState == NodeExplorationState.Exploring)
            {
                node.SetExplorationState(NodeExplorationState.Explored);
                hexGameObjectMap[node].SetNodeComplete();
                _hexGrid.RevealSurroundingNodes(node, gridVisibility);
                GenerateGridVisuals();
            }
        }
        
        public void MarkNodeAsExplored(HexNode node)
        {
            if (node.ExplorationState == NodeExplorationState.Revealed)
            {
                node.SetExplorationState(NodeExplorationState.Explored);
                hexGameObjectMap[node].MarkExploredVisual();
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

        private Sprite GetImageByNodeType(NodeType hexNodeNodeType)
        {
            switch (hexNodeNodeType)
            {
                case NodeType.Empty: return emptyNodeSprite;
                case NodeType.Resource: return resourceNodeSprite;
                case NodeType.Enemy: return enemyNodeSprite;
                case NodeType.Campfire: return campfireNodeSprite;
                case NodeType.Obstacle: return obstacleNodeSprite;
                case NodeType.Boss: return bossNodeSprite;
                default: return emptyNodeSprite;
            }
        }

        private HexNodeDisplay GenerateNodeVisual(GameObject newNode, HexNode hexNode)
        {
            HexNodeDisplay hexNodeDisplay = newNode.GetComponent<HexNodeDisplay>();
            hexNodeDisplay.SetImage(GetImageByNodeType(hexNode.NodeType));
            hexNodeDisplay.HexNode = hexNode;
            hexNodeDisplay.OnNodeClicked.AddListener(OnClickedOnNode);
            hexNodeDisplay.OnNodeEnter.AddListener(OnHoverOnNode);
            hexNodeDisplay.OnNodeLeave.AddListener(OnLeaveNode);

            switch (hexNode.ExplorationState)
            {
                case NodeExplorationState.Revealed:
                    // Just revealed, no special visuals needed
                    break;
                case NodeExplorationState.Explored:
                    hexNodeDisplay.SetNodeComplete();
                    break;
            }
            return hexNodeDisplay;
        }
    }
}