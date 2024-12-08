using System.Collections.Generic;
using UnityEngine;

namespace _Script.Map.Hexagon_Graph
{
    public class MapExplorer : MonoBehaviour
    {
        [Header("Grid Settings")]
        public float hexSize = 0.5f; // Hex size
        public int gridRadius = 5;  // Grid radius
        public GameObject hexPrefab; // Hex prefab

        [Header("Pathfinding Settings")]
        public Color pathColor = Color.green;
        public Color startColor = Color.blue;
        public Color goalColor = Color.red;

        private HexGrid hexGrid;
        private Dictionary<HexNode, GameObject> hexGameObjectMap = new Dictionary<HexNode, GameObject>();
        private List<HexNode> path = new List<HexNode>();
        
        private List<HexNodeVisual> highlightedHexes = new List<HexNodeVisual>();

        
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
            hexGrid = new HexGrid(gridRadius, hexSize, new GridConfiguration(hexSize));

            // Generate visual representations
            GenerateGridVisuals();
        }
        
        

        private void HighlightHex(HexNode hexNode, Color color)
        {
            if (hexGameObjectMap.TryGetValue(hexNode, out GameObject newNode))
            {
                newNode.GetComponent<SpriteRenderer>().color = color;
            }
        }
        
        
        private void GenerateGridVisuals()
        {
            foreach (HexNode hexNode in hexGrid.GetAllHexNodes())
            {
                Vector2 position = hexNode.CubeToWorldPosition2D(hexSize);
                GameObject newNode = Instantiate(hexPrefab, position, Quaternion.identity, this.transform);
                
                // Set HexBehaviour
                HexNodeVisual hexNodeVisual = newNode.GetComponent<HexNodeVisual>();
                
                hexNodeVisual.SetImage(GetImageByNodeType(hexNode.NodeType));
                hexNodeVisual.HexNode = hexNode;
                hexNodeVisual.OnNodeClicked.AddListener(OnClickedOnNode);
                hexNodeVisual.OnNodeEnter.AddListener(OnHoverOnNode);
                hexNodeVisual.OnNodeLeave.AddListener(OnLeaveNode);
                

                // Store in the map
                hexGameObjectMap.Add(hexNode, newNode);
            }
        }
        
        //on disable
        private void OnDisable()
        {
 
        }
        
        private void OnClickedOnNode(INodeHandle handle)
        {
            Debug.Log("Clicked on node: " + handle.GetPosition());
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
