using System;
using System.Collections.Generic;
using _Script.Managers;
using UnityEngine;

namespace _Script.Map.Hexagon_Graph
{
    public class MapExplorer : MonoBehaviour
    {
        [Header("Grid Settings")]
        public float hexSize = 0.5f; // Hex size
        public int gridRadius = 5;  // Grid radius
        public GameObject hexPrefab; // Hex prefab

        public GameObject mapGrid;
        
        [Header("Pathfinding Settings")]
        public Color pathColor = Color.green;
        public Color startColor = Color.blue;
        public Color goalColor = Color.red;

        private HexGrid _hexGrid;
        private Dictionary<HexNode, HexNodeDisplay> hexGameObjectMap = new Dictionary<HexNode, HexNodeDisplay>();
        private List<HexNode> path = new List<HexNode>();
        
        private List<HexNodeDisplay> _highlightedHexes = new List<HexNodeDisplay>();

        
        [SerializeField] private Sprite emptyNodeSprite;
        [SerializeField] private Sprite resourceNodeSprite;
        [SerializeField] private Sprite enemyNodeSprite;
        [SerializeField] private Sprite campfireNodeSprite;
        [SerializeField] private Sprite obstacleNodeSprite;
        [SerializeField] private Sprite bossNodeSprite;
        
        private HexNode startHex;
        private HexNode goalHex;

        private void Start()
        {
            // Initialize the grid
            _hexGrid = new HexGrid(gridRadius, hexSize, new GridConfiguration(hexSize));
            var spawnPoint = _hexGrid.GenerateSpawnPoint();
            startHex = _hexGrid.GetHexNode(spawnPoint.x, spawnPoint.y, spawnPoint.z);
            
            //highlight the player
            
            
            // Generate visual representations
            GenerateGridVisuals();
            hexGameObjectMap[startHex].SetNodeComplete();

        }

        public void Update()
        {
            if(Input.GetKeyDown(KeyCode.M))
            {
                ToggleVisibility();
            }
        }

        private void ToggleVisibility()
        {
            mapGrid.SetActive(!mapGrid.activeSelf);
        }
        
        
        private void GenerateGridVisuals()
        {
            foreach (HexNode hexNode in _hexGrid.GetAllHexNodes())
            {
                Vector2 position = hexNode.CubeToWorldPosition2D(hexSize);
                GameObject newNode = Instantiate(hexPrefab, position, Quaternion.identity, mapGrid.transform);
                
                // Set HexBehaviour
                HexNodeDisplay hexNodeDisplay = newNode.GetComponent<HexNodeDisplay>();
                
                hexNodeDisplay.SetImage(GetImageByNodeType(hexNode.NodeType));
                hexNodeDisplay.HexNode = hexNode;
                hexNodeDisplay.OnNodeClicked.AddListener(OnClickedOnNode);
                hexNodeDisplay.OnNodeEnter.AddListener(OnHoverOnNode);
                hexNodeDisplay.OnNodeLeave.AddListener(OnLeaveNode);
                // Store in the map
                hexGameObjectMap.Add(hexNode, hexNodeDisplay);
            }
            
            //highlight the player 
        }
        
        private void OnClickedOnNode(INodeHandle handle)
        {
            var node = _hexGrid.GetHexNode(handle.GetPosition().x, handle.GetPosition().y, handle.GetPosition().z);
            
            //check if the node is adjacent to the player

            if (_hexGrid.IsAdjacentToPlayer(node))
            {
                
                handle.SetNodeComplete();
                _hexGrid.MovePlayer(node);
                Debug.Log("Player has moved to the new node");
            }
            
            
            Debug.Log($"Clicked on node: {node.NodeType} + currently disabled loading scenes");
            //GameManager.Instance.LoadMap(node.MapNode);
        }
        
        private void OnHoverOnNode(INodeHandle handle)
        {
            handle.Highlight(true);
        }
        
        private void OnLeaveNode(INodeHandle handle)
        {
            handle.Highlight(false);
        }
        

        /**
         * Get the image for a node type
         */
        private Sprite GetImageByNodeType(NodeType hexNodeNodeType)
        {
            switch (hexNodeNodeType)
            {
                case NodeType.Empty:
                    return emptyNodeSprite;
                case NodeType.Resource:
                    return resourceNodeSprite;
                case NodeType.Enemy:
                    return enemyNodeSprite;
                case NodeType.Campfire:
                    return campfireNodeSprite;
                case NodeType.Obstacle:
                    return obstacleNodeSprite;
                case NodeType.Boss:
                    return bossNodeSprite;
                default:
                    return emptyNodeSprite;
            }
        }
    }
}
