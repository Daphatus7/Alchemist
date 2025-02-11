using System;
using System.Collections.Generic;
using _Script.Character.PlayerRank;
using _Script.Managers;
using _Script.Map.WorldMap;
using _Script.Quest;
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


        [SerializeField] private float hexSize = 0.5f;
        [SerializeField] private int gridRadius = 20;
        [SerializeField] private int gridVisibility = 2;
        [SerializeField] private bool debug;


        public void SubscribeToNodeChange(Action<HexNode> action)
        {
            HexGrid.OnNodeChanged += action;
        }
        
        public void UnsubscribeFromNodeChange(Action<HexNode> action)
        {
            HexGrid.OnNodeChanged -= action;
        }
        
        /// <summary>
        /// Initializes the hex grid, sets the spawn point, and reveals the initial nodes.
        /// </summary>
        public void Start()
        {
            InitializeGrid();
        }
        
        public void GeneratePathForQuest(GuildQuestDefinition quest)
        {
            var path = CreatePath(HexGrid.GenerateNodeAtLevel(0), HexGrid.GenerateNodeAtLevel(7));
            SetDifficultyOfNodes(path, GetMapDifficulty(quest.questRank));
        }
        
        private int GetMapDifficulty(PlayerRankEnum questRank)
        {
            return questRank switch
            {
                PlayerRankEnum.F => 1, 
                PlayerRankEnum.E => 2,
                PlayerRankEnum.D => 3,
                PlayerRankEnum.C => 4,
                PlayerRankEnum.B => 5,
                PlayerRankEnum.A => 6,
                PlayerRankEnum.S => 7,
                _ => 1
            };
        }
        
        private void InitializeGrid()
        {
            HexGrid = new HexGrid(gridRadius, new GridConfiguration(hexSize));
            Vector3Int spawnPoint = HexGrid.GenerateSpawnPoint();
            HexGrid.RevealHexNodeInRange(spawnPoint.x, spawnPoint.y, spawnPoint.z, gridVisibility);
            StartHex = HexGrid.GetHexNode(spawnPoint.x, spawnPoint.y, spawnPoint.z);
            //move player to start hex
            HexGrid.MovePlayer(StartHex);
        }
        
        private List<HexNode> CreatePath(HexNode start, HexNode end)
        {
            // Create a path from the start node to the end node.
            var path = HexGrid.FindPath(start, end);
            if (path != null)
            {
                foreach (var node in path)
                {
                    node.SetExplorationState(NodeExplorationState.Revealed);
                }
            }
            return path;
        }
        
        private void SetDifficultyOfNodes(List<HexNode> path, float difficulty)
        {
            // Set the difficulty of the nodes based on the path.
            foreach (var node in path)
            {
                node.Difficulty = difficulty;
            }
        }
        
        /// <summary>
        /// Tries to explore the node based on game rules.
        /// </summary>
        public void TryExploreNode(HexNode node)
        {
            if (node == null) return;

            if (HexGrid.IsAdjacentToPlayer(node) && node.ExplorationState == NodeExplorationState.Revealed)
            { 
                Debug.Log("Exploring node." + node.Position);
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
            // Move player to the node and reveal surrounding nodes
            HexGrid.MovePlayer(node);
            node.SetExplorationState(NodeExplorationState.Exploring);
            MarkCurrentNodeAsExplored(node);
            HexGrid.RevealHexNodeInRange(node.Position.x, node.Position.y, node.Position.z, gridVisibility);

            
            if (debug)
            {
                // Debug: stay in map view after exploring
            }
            else
            {
                // Normal: switch back to game view after exploring
                MapExplorerView.Instance.HideUI();
                GameManager.Instance.LoadSelectedScene(node.NodeData);
            }
        }

        /// <summary>
        /// Marks the current (exploring) node as fully explored.
        /// </summary>
        public void MarkCurrentNodeAsExplored(HexNode node)
        {
            if (node.ExplorationState == NodeExplorationState.Exploring)
            {
                node.SetExplorationState(NodeExplorationState.Explored);
                HexGrid.NotifyNodeChanged(node);
            }
        }
        
        public void MarkCurrentNodeAsExplored()
        {
            var node = HexGrid.GetHexNode(PlayerPosition.x, PlayerPosition.y, PlayerPosition.z);
            if (node != null)
                MarkCurrentNodeAsExplored(node);
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
