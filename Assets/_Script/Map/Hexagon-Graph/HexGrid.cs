using System;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Map.Hexagon_Graph
{
    public class HexGrid
    {
        private Dictionary<(int x, int y, int z), HexNode> hexNodes = new Dictionary<(int, int, int), HexNode>();
        private Dictionary<(int x, int y, int z), List<HexNode>> _neighborsCache = new Dictionary<(int, int, int), List<HexNode>>();

        private const int ViewRadius = 2;
        private readonly int _gridRadius;
        public float HexSize;
        private readonly GridConfiguration _gridConfiguration;

        private int _weightSum;
        private int _obstacleWeight;
        private int _resourceWeight;
        private int _enemyWeight;
        private int _campfireWeight;
        private int _bossWeight;

        private Vector3Int _playerPosition;

        public HexGrid(int gridRadius, float hexSize, GridConfiguration gridConfiguration)
        {
            _gridRadius = gridRadius;
            HexSize = hexSize;
            _gridConfiguration = gridConfiguration;
            CalculateWeightDistribution();
            GenerateGrid();
            PrecomputeNeighbors();
        }

        private void CalculateWeightDistribution()
        {
            _obstacleWeight = _gridConfiguration.ObstacleWeight;
            _resourceWeight = _obstacleWeight + _gridConfiguration.ResourceWeight;
            _enemyWeight = _resourceWeight + _gridConfiguration.EnemyWeight;
            _campfireWeight = _enemyWeight + _gridConfiguration.CampfireWeight;
            _bossWeight = _campfireWeight + _gridConfiguration.BossWeight;
            _weightSum = _bossWeight;
        }

        private NodeType GenerateHexType()
        {
            int rand = UnityEngine.Random.Range(0, _weightSum);
            if (rand < _obstacleWeight) return NodeType.Obstacle;
            if (rand < _resourceWeight) return NodeType.Resource;
            if (rand < _enemyWeight) return NodeType.Enemy;
            if (rand < _campfireWeight) return NodeType.Campfire;
            return NodeType.Boss;
        }

        private void GenerateGrid()
        {
            hexNodes.Clear();
            for (int x = -_gridRadius; x <= _gridRadius; x++)
            {
                for (int y = Mathf.Max(-_gridRadius, -x - _gridRadius); y <= Mathf.Min(_gridRadius, -x + _gridRadius); y++)
                {
                    int z = -x - y;
                    NodeType newNodeType = GenerateHexType();
                    HexNode hexNode = new HexNode(new Vector3Int(x, y, z), newNodeType, new MapNode());
                    hexNodes.Add((x, y, z), hexNode);
                }
            }
        }

        private void PrecomputeNeighbors()
        {
            foreach (var kvp in hexNodes)
            {
                (int x, int y, int z) = kvp.Key;
                List<HexNode> neighbors = new List<HexNode>(6);
                foreach (var dir in HexNode.directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    int nz = z + dir.z;
                    if (hexNodes.TryGetValue((nx, ny, nz), out var neighborNode))
                    {
                        neighbors.Add(neighborNode);
                    }
                }
                _neighborsCache[kvp.Key] = neighbors;
            }
        }

        public HexNode GetHexNode(int x, int y, int z)
        {
            hexNodes.TryGetValue((x, y, z), out HexNode hexNode);
            return hexNode;
        }

        public IEnumerable<HexNode> GetAllHexNodes()
        {
            return hexNodes.Values;
        }

        public IEnumerable<HexNode> GetAllVisibleHexNodes()
        {
            foreach (var node in hexNodes.Values)
            {
                if (node.ExplorationState != NodeExplorationState.Unrevealed)
                {
                    yield return node;
                }
            }
        }

        public List<HexNode> GetHexNodesInView(HexNode playerNode)
        {
            List<HexNode> nodesInView = new List<HexNode>();
            int radius = ViewRadius;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
                {
                    int dz = -dx - dy;
                    int x = playerNode.Position.x + dx;
                    int y = playerNode.Position.y + dy;
                    int z = playerNode.Position.z + dz;

                    if (hexNodes.TryGetValue((x, y, z), out var hexNode))
                    {
                        nodesInView.Add(hexNode);
                    }
                }
            }
            return nodesInView;
        }

        private class NodeComparer : IComparer<HexNode>
        {
            public int Compare(HexNode a, HexNode b)
            {
                int result = a.fCost.CompareTo(b.fCost);
                if (result == 0)
                    result = a.hCost.CompareTo(b.hCost);
                return result;
            }
        }

        public List<HexNode> FindPath(HexNode startNode, HexNode goalNode)
        {
            ResetPathfindingData();

            var openSet = new SortedSet<HexNode>(new NodeComparer());
            HashSet<HexNode> closedSet = new HashSet<HexNode>();

            startNode.gCost = 0;
            startNode.hCost = startNode.Distance(goalNode);
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                HexNode currentNode = openSet.Min;
                openSet.Remove(currentNode);

                if (currentNode == goalNode)
                {
                    return RetracePath(startNode, currentNode);
                }

                closedSet.Add(currentNode);

                List<HexNode> neighbors = _neighborsCache[(currentNode.Position.x, currentNode.Position.y, currentNode.Position.z)];
                foreach (HexNode neighbor in neighbors)
                {
                    if (neighbor.IsBlocked || closedSet.Contains(neighbor))
                        continue;

                    int tentativeGCost = currentNode.gCost + 1;
                    if (tentativeGCost < neighbor.gCost)
                    {
                        if (openSet.Contains(neighbor))
                            openSet.Remove(neighbor);

                        neighbor.gCost = tentativeGCost;
                        neighbor.hCost = neighbor.Distance(goalNode);
                        neighbor.Parent = currentNode;
                        openSet.Add(neighbor);
                    }
                }
            }

            Debug.LogError("Path not found");
            return null;
        }

        private List<HexNode> RetracePath(HexNode startNode, HexNode endNode)
        {
            List<HexNode> path = new List<HexNode>();
            HexNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }
            path.Add(startNode);
            path.Reverse();
            return path;
        }

        private void ResetPathfindingData()
        {
            foreach (var node in hexNodes.Values)
            {
                node.gCost = int.MaxValue;
                node.hCost = 0;
                node.Parent = null;
            }
        }

        public bool IsAdjacentToPlayer(HexNode node)
        {
            var playerNode = GetHexNode(_playerPosition.x, _playerPosition.y, _playerPosition.z);
            if (playerNode == null)
            {
                Debug.LogError("Player node not found in the grid.");
                return false;
            }

            List<HexNode> playerNeighbors = _neighborsCache[(_playerPosition.x, _playerPosition.y, _playerPosition.z)];
            return playerNeighbors.Contains(node);
        }

        public void MovePlayer(HexNode node)
        {
            _playerPosition = node.Position;
        }

        public Vector3Int GenerateSpawnPoint()
        {
            HexNode startNode = GetHexNode(0, 0, 0);
            if (startNode != null && startNode.NodeType != NodeType.Obstacle)
            {
                _playerPosition = startNode.Position;
                return _playerPosition;
            }

            int searchRadius = _gridRadius;
            for (int r = 1; r <= searchRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = Mathf.Max(-r, -dx - r); dy <= Mathf.Min(r, -dx + r); dy++)
                    {
                        int dz = -dx - dy;
                        if (hexNodes.TryGetValue((dx, dy, dz), out var node) && node.NodeType != NodeType.Obstacle)
                        {
                            _playerPosition = node.Position;
                            return _playerPosition;
                        }
                    }
                }
            }

            Debug.LogError("No valid spawn point found.");
            _playerPosition = Vector3Int.zero;
            return _playerPosition;
        }

        public Vector3Int SpawnPlayer()
        {
            return _playerPosition;
        }

        // Reveal a single node
        public void RevealHexNode(int x, int y, int z)
        {
            var node = GetHexNode(x, y, z);
            if (node != null && node.ExplorationState == NodeExplorationState.Unrevealed)
            {
                node.SetExplorationState(NodeExplorationState.Revealed);
            }
        }

        // Reveal nodes around a given node position within the default view radius
        public void RevealHexNodeInRange(int x, int y, int z, int radius)
        {
            var centerNode = GetHexNode(x, y, z);
            if (centerNode == null) return;

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
                {
                    int dz = -dx - dy;
                    int nx = centerNode.Position.x + dx;
                    int ny = centerNode.Position.y + dy;
                    int nz = centerNode.Position.z + dz;

                    var node = GetHexNode(nx, ny, nz);
                    if (node != null && node.ExplorationState == NodeExplorationState.Unrevealed)
                    {
                        node.SetExplorationState(NodeExplorationState.Revealed);
                    }
                }
            }
        }

        // Reveal nodes around a given node position within a specified radius
        public void RevealSurroundingNodes(HexNode centerNode, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = Mathf.Max(-radius, -dx - radius); dy <= Mathf.Min(radius, -dx + radius); dy++)
                {
                    int dz = -dx - dy;
                    int nx = centerNode.Position.x + dx;
                    int ny = centerNode.Position.y + dy;
                    int nz = centerNode.Position.z + dz;

                    var node = GetHexNode(nx, ny, nz);
                    if (node != null && node.ExplorationState == NodeExplorationState.Unrevealed)
                    {
                        node.SetExplorationState(NodeExplorationState.Revealed);
                    }
                }
            }
        }
    }
}