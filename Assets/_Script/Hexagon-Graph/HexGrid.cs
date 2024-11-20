using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Hexagon_Graph
{
    public class HexGrid
    {
        // Grid data: key is (x, y, z), value is HexNode
        private Dictionary<(int x, int y, int z), HexNode> hexNodes = new Dictionary<(int, int, int), HexNode>();
        private int viewRadius = 5;
        // Grid radius
        public int gridRadius;
        

        // Hex size
        public float hexSize;
        private GridConfiguration gridConfiguration;

        private int weightSum;
        private int obstacleWeight;
        private int resourceWeight;
        private int enemyWeight;
        private int campfireWeight;
        private int bossWeight;
        
        //Player
        private Vector3Int playerPosition;
    
        public Vector3Int SpawnPlayer()
        {
            playerPosition = new Vector3Int(0, 0, 0);
            return playerPosition;
        }
        
        // Constructor
        public HexGrid(int gridRadius, float hexSize, GridConfiguration gridConfiguration)
        {
            this.gridRadius = gridRadius;
            this.hexSize = hexSize;
            this.gridConfiguration = gridConfiguration;
            CalculateWeightDistribution();
            GenerateGrid();
        }
    
        private void CalculateWeightDistribution()
        {
            obstacleWeight = gridConfiguration.ObstacleWeight;
            resourceWeight = obstacleWeight + gridConfiguration.ResourceWeight;
            enemyWeight = resourceWeight + gridConfiguration.EnemyWeight;
            campfireWeight = enemyWeight + gridConfiguration.CampfireWeight;
            bossWeight = campfireWeight + gridConfiguration.BossWeight;
            weightSum = bossWeight;
        }
    

        private HexNode GetOrCreateHexNode(int x, int y, int z)
        {
            var key = (x, y, z);
            if (!hexNodes.TryGetValue(key, out HexNode hexNode))
            {
                // Generate a new HexNode
                hexNode = GenerateHexNode(x, y, z);
                hexNodes.Add(key, hexNode);
            }
            return hexNode;
        }
    
        // generate a hex node
        private HexNode GenerateHexNode(int x, int y, int z)
        {
            // get existing neighbors
            List<HexNode> neighbors = GetExistingNeighbors(x, y, z);

            NodeType hexType;
            if (neighbors.Exists(n => n.NodeType != NodeType.Obstacle))
            {
                // if any neighbor is not an obstacle, generate a random hex type
                hexType = GenerateHexType();
            }
            else
            {
                // if all neighbors are obstacles, generate an empty hex
                hexType = NodeType.Empty;
            }

            return new HexNode(new Vector3Int(x, y, z), hexType);
        }
    

        // generate a random hex type
        private NodeType GenerateHexType()
        {
            // generate a random float between 0 and 1
            int rand = UnityEngine.Random.Range(0, weightSum);
            if (rand < obstacleWeight)
            {
                return NodeType.Obstacle;
            }
            if (rand < resourceWeight)
            {
                return NodeType.Resource;
            }
            if (rand < enemyWeight)
            {
                return NodeType.Enemy;
            }
            if (rand < campfireWeight)
            {
                return NodeType.Campfire;
            }
            return NodeType.Boss;
        }


        // get hex nodes in view
        public List<HexNode> GetHexNodesInView(HexNode playerNode)
        {
            List<HexNode> nodesInView = new List<HexNode>();
            int radius = viewRadius;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
                {
                    int dz = -dx - dy;
                    int x = playerNode.Position.x + dx;
                    int y = playerNode.Position.y + dy;
                    int z = playerNode.Position.z + dz;

                    HexNode hexNode = GetOrCreateHexNode(x, y, z);
                    nodesInView.Add(hexNode);
                }
            }
            return nodesInView;
        }

        // Generate the hex grid
        private void GenerateGrid()
        {
            hexNodes.Clear();

            for (int x = -gridRadius; x <= gridRadius; x++)
            {
                for (int y = Mathf.Max(-gridRadius, -x - gridRadius); y <= Mathf.Min(gridRadius, -x + gridRadius); y++)
                {
                    int z = -x - y;
                    NodeType newNodeType = GenerateHexType();
                    HexNode hexNode = new HexNode(new Vector3Int(x, y, z), newNodeType);
                    // Randomly block some hexes
                    hexNodes.Add((x, y, z), hexNode);
                }
            }
        }
        
        // Get a HexNode by coordinates
        public HexNode GetHexNode(int x, int y, int z)
        {
            hexNodes.TryGetValue((x, y, z), out HexNode hexNode);
            return hexNode;
        }

        // Get all HexNodes
        public IEnumerable<HexNode> GetAllHexNodes()
        {
            return hexNodes.Values;
        }

        // A* pathfinding algorithm
        public List<HexNode> FindPath(HexNode startNode, HexNode goalNode)
        {
            // Reset pathfinding data
            ResetPathfindingData();

            List<HexNode> openSet = new List<HexNode>();
            HashSet<HexNode> closedSet = new HashSet<HexNode>();

            startNode.gCost = 0;
            startNode.hCost = startNode.Distance(goalNode);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                // Get the node with the lowest fCost
                HexNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost ||
                        (openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost))
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode.Equals(goalNode))
                {
                    // Path found
                    return RetracePath(startNode, currentNode);
                }

                foreach (HexNode neighbor in GetNeighbors(currentNode))
                {
                    if (neighbor.isBlocked || closedSet.Contains(neighbor))
                        continue;

                    int tentativeGCost = currentNode.gCost + 1; // Assuming cost between nodes is 1
                    bool inOpenSet = openSet.Contains(neighbor);

                    if (!inOpenSet || tentativeGCost < neighbor.gCost)
                    {
                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = neighbor.Distance(goalNode);
                        neighbor.parent = currentNode;

                        if (!inOpenSet)
                            openSet.Add(neighbor);
                    }
                }
            }

            Debug.LogError("Path not found");
            // Failed to find a path
            return null;
        }

        // Retrace the path from the endNode to the startNode
        private List<HexNode> RetracePath(HexNode startNode, HexNode endNode)
        {
            List<HexNode> path = new List<HexNode>();
            HexNode currentNode = endNode;

            while (!currentNode.Equals(startNode))
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Add(startNode); // Add the start node
            path.Reverse();
            return path;
        }

        // Reset pathfinding data
        private void ResetPathfindingData()
        {
            foreach (var node in hexNodes.Values)
            {
                node.gCost = int.MaxValue;
                node.hCost = 0;
                node.parent = null;
            }
        }
    
        // Get neighbors from the grid
        public List<HexNode> GetNeighbors(HexNode node)
        {
            List<HexNode> neighbors = new List<HexNode>();
            foreach (var dir in HexNode.directions)
            {
                int neighborX = node.Position.x + dir.x;
                int neighborY = node.Position.y + dir.y;
                int neighborZ = node.Position.z + dir.z;

                HexNode neighborNode = GetHexNode(neighborX, neighborY, neighborZ);
                if (neighborNode != null)
                {
                    neighbors.Add(neighborNode);
                }
            }
            return neighbors;
        }
    
    
        private List<HexNode> GetExistingNeighbors(int x, int y, int z)
        {
            List<HexNode> neighbors = new List<HexNode>();
            foreach (var dir in HexNode.directions)
            {
                int nx = x + dir.x;
                int ny = y + dir.y;
                int nz = z + dir.z;
                var key = (nx, ny, nz);

                if (hexNodes.TryGetValue(key, out HexNode neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }
            return neighbors;
        }
    }
}
