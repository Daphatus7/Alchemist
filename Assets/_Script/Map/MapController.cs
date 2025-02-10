using System;
using _Script.Map.WorldMap;
using UnityEngine;


// Only for basic types such as Vector3Int. (Replace if needed with your own math types.)
// Assuming HexGrid, HexNode, NodeExplorationState, etc. are defined here.

namespace _Script.Map
{
    /// <summary>
    /// Contains the complete map logic independent of any UI.
    /// </summary>
    public class MapController : Singleton<MapController>
    {
        public HexGrid HexGrid { get; private set; }
        public HexNode StartHex { get; private set; }
        public Vector3Int PlayerPosition => HexGrid.PlayerPosition;

        // Raised whenever a HexNode’s state changes (for example, when a node is explored)
        public event Action<HexNode> OnNodeChanged;

        [SerializeField] private int _gridRadius;
        [SerializeField] private float hexSize = 0.5f;
        [SerializeField] private int gridRadius = 20;
        [SerializeField] private int gridVisibility = 2;
        [SerializeField] private bool _debug;
        

        /// <summary>
        /// Initializes the hex grid, sets the spawn point, and reveals the initial nodes.
        /// </summary>
        public void Start()
        {
            InitializeGrid();
        }

        private void InitializeGrid()
        {
            HexGrid = new HexGrid(_gridRadius, new GridConfiguration(hexSize));
            Vector3Int spawnPoint = HexGrid.GenerateSpawnPoint();
            HexGrid.RevealHexNodeInRange(spawnPoint.x, spawnPoint.y, spawnPoint.z, gridVisibility);
            StartHex = HexGrid.GetHexNode(spawnPoint.x, spawnPoint.y, spawnPoint.z);
            //move player to start hex
            HexGrid.MovePlayer(StartHex);
            // Subscribe to grid events and propagate them via our own event.
            HexGrid.OnNodeChanged += HandleGridNodeChanged;
            

        }

        private void OnDestroy()
        {
            if (HexGrid != null)
                HexGrid.OnNodeChanged -= HandleGridNodeChanged;
        }

        private void HandleGridNodeChanged(HexNode node)
        {
            OnNodeChanged?.Invoke(node);
        }

        /// <summary>
        /// Tries to explore the node based on game rules.
        /// </summary>
        public void TryExploreNode(HexNode node)
        {
            if (node == null) return;

            if (HexGrid.IsAdjacentToPlayer(node) && node.ExplorationState == NodeExplorationState.Revealed)
            { 
                ExploreNode(node);
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

        private void ExploreNode(HexNode node)
        {
            HexGrid.MovePlayer(node);
            node.SetExplorationState(NodeExplorationState.Exploring);

            if (_debug)
            {
                MarkCurrentNodeAsExplored();
            }
            else
            {
                // In normal mode you might trigger a scene change here.
                // For example: GameManager.Instance.LoadSelectedScene(node.NodeData);
            }
        }

        /// <summary>
        /// Marks the current (exploring) node as fully explored.
        /// </summary>
        public void MarkCurrentNodeAsExplored()
        {
            var node = HexGrid.GetHexNode(PlayerPosition.x, PlayerPosition.y, PlayerPosition.z);
            if (node.ExplorationState == NodeExplorationState.Exploring)
            {
                node.SetExplorationState(NodeExplorationState.Explored);
                HexGrid.RevealSurroundingNodes(node, gridVisibility);
                OnNodeChanged?.Invoke(node);
            }
        }

        /// <summary>
        /// Resets the grid to its initial state.
        /// </summary>
        public void ResetGrid()
        {
            HexGrid.DebugResetGrid();
            InitializeGrid();
        }
    }
}
