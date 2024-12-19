using System;
using System.Collections.Generic;
using _Script.Map.WorldMap.MapNode;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map.WorldMap
{
    public class HexGrid
    {
        private Dictionary<(int x, int y, int z), HexNode> hexNodes = new Dictionary<(int, int, int), HexNode>();
        private Dictionary<(int x, int y, int z), List<HexNode>> _neighborsCache = new Dictionary<(int, int, int), List<HexNode>>();

        private const int ViewRadius = 2;
        private readonly int _gridRadius;
        private readonly GridConfiguration _gridConfiguration;

        private Vector3Int _playerPosition;
        public Vector3Int PlayerPosition => _playerPosition;

        public event Action<HexNode> OnNodeChanged;

        private void NotifyNodeChanged(HexNode node)
        {
            OnNodeChanged?.Invoke(node);
        }

        public HexGrid(int gridRadius, GridConfiguration gridConfiguration)
        {
            _gridRadius = gridRadius;
            _gridConfiguration = gridConfiguration;

            GenerateGrid();
            PrecomputeNeighbors();
            GenerateBranchingStreams();
        }

        private NodeType GenerateHexType()
        {
            return _gridConfiguration.GetRandomType();
        }

        public void DebugResetGrid()
        {
            // Clear data structures
            hexNodes.Clear();
            _neighborsCache.Clear();

            // Regenerate grid and neighbors
            GenerateGrid();
            PrecomputeNeighbors();

            // Optionally regenerate branching streams if needed
            GenerateBranchingStreams();

            // Notify listeners that nodes have updated
            foreach (var node in hexNodes.Values)
            {
                OnNodeChanged?.Invoke(node);
            }

            Debug.Log("Hex Grid has been reset.");
        }

        private bool IsAtBoundary(Vector3Int pos)
        {
            int dist = Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y), Mathf.Abs(pos.z));
            return dist == _gridRadius;
        }

        private void GenerateBranchingStreams()
        {
            HexNode startNode = GetHexNode(0,0,0);

            var firstForkEnds = Fork(Vector3.right, startNode.Position, 3, 3);

            var queue = new Queue<HexNode>();

            // When adding initial end nodes to the queue, check if they are at the boundary
            foreach (var end in firstForkEnds)
            {
                if (!IsAtBoundary(end.Position))
                    queue.Enqueue(end);
            }

            // Only continue generating new paths if nodes haven't reached the boundary
            while (queue.Count > 0 && queue.Count < 30)
            {
                var end = queue.Dequeue();
                end.NodeType = NodeType.Bonfire;
                var direction = DirectionVector(end.Position, startNode.Position);
                var nextForkEnds = Fork(direction, end.Position, 4, RandomNumberOfFork(end.NodeLevel, 2));

                // Check if newly generated end nodes are at the boundary
                foreach (var nextEnd in nextForkEnds)
                {
                    if (!IsAtBoundary(nextEnd.Position))
                        queue.Enqueue(nextEnd);
                }
            }
        }

        private int RandomNumberOfFork(int level, int max)
        {
            float v = 1f / (level + 1);
            float p = v * v;
            return UnityEngine.Random.value < p ? 2 : 1;
        }

        private Vector3 DirectionVector(Vector3Int end, Vector3Int start)
        {
            return new Vector3(end.x - start.x, end.y - start.y, end.z - start.z);
        }

        /// <summary>
        /// Generate forks from a given direction and start point without restricting to 60-degree steps.
        /// The given direction is an approximate cube coordinate direction vector.
        /// We add a small random angle (e.g., ±30 degrees), and then map it back to cube coordinates.
        /// </summary>
        private List<HexNode> Fork(Vector3 direction, Vector3Int start, int distance, int numberOfForks)
        {
            var ends = new List<HexNode>();
            HexNode startNode = GetHexNode(start.x, start.y, start.z);
            if (startNode == null) return ends;

            for (int i = 0; i < numberOfForks; i++)
            {
                // Randomly choose an angle offset between -90 and 90 degrees
                bool randomBool = UnityEngine.Random.value > 0.5f;
                float angleDeg = randomBool 
                    ? UnityEngine.Random.Range(-90f, -60f) 
                    : UnityEngine.Random.Range(60f, 90f);

                // Normalize direction and treat x,z as a 2D plane
                Vector3 dirNorm = direction.normalized;
                float q = dirNorm.x;
                float r = dirNorm.z;

                // Rotate (q,r) in the 2D plane by angleDeg
                Vector2 rotated = Rotate2D(new Vector2(q, r), angleDeg * Mathf.Deg2Rad);

                // Scale the rotated vector to the given distance
                Vector2 final2D = rotated.normalized * distance;

                // Convert final2D(q,r) back to cube coordinates
                float fx = final2D.x;
                float fz = final2D.y;
                float fy = -fx - fz;

                // Compute final node coordinates based on start
                float finalX = start.x + fx;
                float finalY = start.y + fy;
                float finalZ = start.z + fz;

                // Cube-round the float coordinates to the nearest hex cell
                Vector3Int endCube = CubeRound(finalX, finalY, finalZ);

                // Find endNode and try to find a path
                HexNode endNode = GetHexNode(endCube.x, endCube.y, endCube.z);
                if (endNode != null)
                {
                    var path = FindPath(startNode, endNode);
                    if (path != null && path.Count > 0)
                    {
                        SetPathNodes(path);
                        ends.Add(endNode);
                    }
                }
            }

            return ends;
        }

        /// <summary>
        /// Rotate a 2D vector by a given angle in radians.
        /// </summary>
        private Vector2 Rotate2D(Vector2 v, float angleRad)
        {
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            float nx = v.x * cos - v.y * sin;
            float ny = v.x * sin + v.y * cos;
            return new Vector2(nx, ny);
        }

        /// <summary>
        /// Round float cube coordinates to the nearest hex cell.
        /// Based on: https://www.redblobgames.com/grids/hexagons/
        /// </summary>
        private Vector3Int CubeRound(float x, float y, float z)
        {
            int rx = Mathf.RoundToInt(x);
            int ry = Mathf.RoundToInt(y);
            int rz = Mathf.RoundToInt(z);

            float dx = Mathf.Abs(rx - x);
            float dy = Mathf.Abs(ry - y);
            float dz = Mathf.Abs(rz - z);

            if (dx > dy && dx > dz)
                rx = -ry - rz;
            else if (dy > dz)
                ry = -rx - rz;
            else
                rz = -rx - ry;

            return new Vector3Int(rx, ry, rz);
        }

        /// <summary>
        /// Find the closest of the six main hex directions to the given 3D direction vector.
        /// </summary>
        private int FindClosestDirectionIndex(Vector3 direction)
        {
            int closestIndex = 0;
            float closestAngle = float.MaxValue;

            Vector3[] directionVectors = {
                new Vector3(1,-1,0), 
                new Vector3(1,0,-1),
                new Vector3(0,1,-1),
                new Vector3(-1,1,0),
                new Vector3(-1,0,1),
                new Vector3(0,-1,1)
            };

            Vector3 dirNorm = direction.normalized;

            for (int i = 0; i < 6; i++)
            {
                float angle = Vector3.Angle(dirNorm, directionVectors[i].normalized);
                if (angle < closestAngle)
                {
                    closestAngle = angle;
                    closestIndex = i;
                }
            }

            return closestIndex;
        }

        private static readonly Vector3Int[] HexDirections = new Vector3Int[]
        {
            new Vector3Int(+1, -1, 0),
            new Vector3Int(+1, 0, -1),
            new Vector3Int(0, +1, -1),
            new Vector3Int(-1, +1, 0),
            new Vector3Int(-1, 0, +1),
            new Vector3Int(0, -1, +1)
        };

        public HexNode GenerateNodeAtLevel(int level)
        {
            for (int x = -_gridRadius; x <= _gridRadius; x++)
            {
                for (int y = Mathf.Max(-_gridRadius, -x - _gridRadius); y <= Mathf.Min(_gridRadius, -x + _gridRadius); y++)
                {
                    int z = -x - y;
                    HexNode checkNode = GetHexNode(x, y, z);
                    if (checkNode != null && checkNode.NodeLevel == level)
                    {
                        return checkNode;
                    }
                }
            }
            return null;
        }

        private void SetPathNodes(List<HexNode> path)
        {
            foreach (var node in path)
            {
                node.NodeType = GenerateHexType();
                NotifyNodeChanged(node);
            }
        }

        private void GenerateGrid()
        {
            hexNodes.Clear();
            for (int x = -_gridRadius; x <= _gridRadius; x++)
            {
                int minY = Mathf.Max(-_gridRadius, -x - _gridRadius);
                int maxY = Mathf.Min(_gridRadius, -x + _gridRadius);
                for (int y = minY; y <= maxY; y++)
                {
                    int z = -x - y;
                    var newNode = new HexNode(new Vector3Int(x, y, z), NodeType.Empty, GenerateNodeData(NodeType.Empty));
                    hexNodes.Add((x, y, z), newNode);
                }
            }
        }

        private NodeData GenerateNodeData(NodeType nodeType)
        {
            return MapNodeFactory.Instance.CreateNode(nodeType, "Resource", 0);
        }

        private void PrecomputeNeighbors()
        {
            foreach (var kvp in hexNodes)
            {
                var key = kvp.Key;
                var neighbors = new List<HexNode>(6);
                foreach (var dir in HexNode.directions)
                {
                    int nx = key.x + dir.x;
                    int ny = key.y + dir.y;
                    int nz = key.z + dir.z;
                    if (hexNodes.TryGetValue((nx, ny, nz), out var neighborNode))
                    {
                        neighbors.Add(neighborNode);
                    }
                }
                _neighborsCache[key] = neighbors;
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
            var nodesInView = new List<HexNode>();
            int radius = ViewRadius;

            for (int dx = -radius; dx <= radius; dx++)
            {
                int startY = Mathf.Max(-radius, -dx - radius);
                int endY = Mathf.Min(radius, -dx + radius);
                int baseX = playerNode.Position.x;
                int baseY = playerNode.Position.y;
                int baseZ = playerNode.Position.z;

                for (int dy = startY; dy <= endY; dy++)
                {
                    int dz = -dx - dy;
                    int x = baseX + dx;
                    int y = baseY + dy;
                    int z = baseZ + dz;

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
            var closedSet = new HashSet<HexNode>();

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

                if (!_neighborsCache.TryGetValue((currentNode.Position.x, currentNode.Position.y, currentNode.Position.z), out var neighbors))
                    continue;

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
            var path = new List<HexNode>();
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

            if (_neighborsCache.TryGetValue((_playerPosition.x, _playerPosition.y, _playerPosition.z), out var playerNeighbors))
            {
                return playerNeighbors.Contains(node);
            }
            return false;
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

        public void RevealHexNode(int x, int y, int z)
        {
            var node = GetHexNode(x, y, z);
            if (node != null && node.ExplorationState == NodeExplorationState.Unrevealed)
            {
                node.SetExplorationState(NodeExplorationState.Revealed);
            }
        }

        public void RevealHexNodeInRange(int x, int y, int z, int radius)
        {
            var centerNode = GetHexNode(x, y, z);
            if (centerNode == null) return;

            for (int dx = -radius; dx <= radius; dx++)
            {
                int startY = Mathf.Max(-radius, -dx - radius);
                int endY = Mathf.Min(radius, -dx + radius);
                for (int dy = startY; dy <= endY; dy++)
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

        public void RevealSurroundingNodes(HexNode centerNode, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int startY = Mathf.Max(-radius, -dx - radius);
                int endY = Mathf.Min(radius, -dx + radius);
                for (int dy = startY; dy <= endY; dy++)
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