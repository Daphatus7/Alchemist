using System;
using System.Collections.Generic;
using _Script.Character.PlayerRank;
using _Script.Map.WorldMap.MapNode;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.Map.WorldMap
{
    /// <summary>
    /// Represents a hexagonal grid of <see cref="HexNode"/> objects.
    /// Provides methods for generating and managing the grid, performing pathfinding,
    /// and handling game-related logic such as player movement and revealing nodes.
    /// </summary>
    public class HexGrid
    {
        /// <summary>
        /// Stores all hex nodes in the grid, keyed by their (x, y, z) coordinates.
        /// </summary>
        private readonly Dictionary<(int x, int y, int z), HexNode> _hexNodes
            = new Dictionary<(int, int, int), HexNode>();

        /// <summary>
        /// Caches each node's neighbors for fast lookups.
        /// The key is a node's (x, y, z) coordinate, and the value is a list of that node's neighbors.
        /// </summary>
        private readonly Dictionary<(int x, int y, int z), List<HexNode>> _neighborsCache
            = new Dictionary<(int, int, int), List<HexNode>>();
        
        private readonly Dictionary<int, List<HexNode>> _nodesByLevel 
            = new Dictionary<int, List<HexNode>>();

        /// <summary>
        /// The radius (in hex steps) in which nodes are considered "in view."
        /// </summary>
        private const int ViewRadius = 10;

        /// <summary>
        /// The overall radius of the hexagonal grid (determines how large the grid is).
        /// </summary>
        private readonly int _gridRadius;

        /// <summary>
        /// A reference to a configuration object used to determine random node types, etc.
        /// </summary>
        private readonly GridConfiguration _gridConfiguration;

        /// <summary>
        /// Tracks the player's current position (x, y, z) within the grid.
        /// </summary>
        private Vector3Int _playerPosition;

        /// <summary>
        /// Public accessor for the player's position within the grid.
        /// </summary>
        public Vector3Int PlayerPosition => _playerPosition;

        /// <summary>
        /// An event that triggers whenever a node's data or type changes,
        /// allowing any interested subscriber to update accordingly.
        /// </summary>
        public event Action<HexNode> OnNodeChanged;

        /// <summary>
        /// Raises the <see cref="OnNodeChanged"/> event for the specified <paramref name="node"/>.
        /// </summary>
        /// <param name="node">The node that changed.</param>
        internal void NotifyNodeChanged(HexNode node)
        {
            OnNodeChanged?.Invoke(node);
        }

        /// <summary>
        /// Creates a new <see cref="HexGrid"/> with the specified radius and grid configuration,
        /// then generates all necessary data (nodes, neighbors, etc.).
        /// </summary>
        /// <param name="gridRadius">The radius of the hex grid.</param>
        /// <param name="gridConfiguration">Configuration for generating node types and data.</param>
        public HexGrid(int gridRadius, GridConfiguration gridConfiguration)
        {
            _gridRadius = gridRadius;
            _gridConfiguration = gridConfiguration;

            GenerateGrid();
            PrecomputeNeighbors();
        }
        
        /// <summary>
        /// Iterates over all nodes in the grid; if a node is not an obstacle and lacks data,
        /// it generates the node data via <see cref="GenerateNodeData"/>.
        /// </summary>
        //obsolete
        [Obsolete]
        private void GenerateDataForAllNodes()
        {
            foreach (var node in _hexNodes)
            {
                node.Value.NodeDataInstance = GenerateNodeData(node.Value.NodeDataInstance.NodeType, PlayerRankEnum.S);
            }
        }

        
        /// <summary>
        /// Returns a random point in cube coordinates that is exactly <paramref name="distance"/> away from <paramref name="center"/>.
        /// </summary>
        /// <param name="center">The central cube coordinate.</param>
        /// <param name="distance">The exact hex distance from the center (must be >= 0).</param>
        /// <returns>A cube coordinate (Vector3Int) exactly <paramref name="distance"/> away from <paramref name="center"/>.</returns>
        public List<Vector3Int> GetARandomPointAt(Vector3Int center, int distance)
        {
            if (distance <= 0)
                return new List<Vector3Int> {center};

            // The total number of points on a hex ring is 6 * distance.
            // Start at one of the ring's corners. Here we use HexDirections[4] arbitrarily.
            Vector3Int start = center + new Vector3Int(
                HexDirections[4].x * distance,
                HexDirections[4].y * distance,
                HexDirections[4].z * distance
            );

            List<Vector3Int> ringPoints = new List<Vector3Int>(6 * distance);
            Vector3Int current = start;

            // There are 6 sides to the hexagon. On each side, we move 'distance' steps.
            for (int side = 0; side < 6; side++)
            {
                for (int step = 0; step < distance; step++)
                {
                    ringPoints.Add(current);
                    // Move one step along the current side direction.
                    current += HexDirections[side];
                }
            }

            // Pick one random point from the ring.
            //int index = Random.Range(0, ringPoints.Count);
            return ringPoints;
        }
        
        /// <summary>
        /// Uses the grid configuration to determine a random node type.
        /// </summary>
        /// <returns>A randomly generated <see cref="NodeType"/>.</returns>
        public NodeType GenerateHexType()
        {
            return _gridConfiguration.GetRandomType();
        }

        /// <summary>
        /// Debugging method that clears out the grid and regenerates it, optionally resetting any streams or 
        /// other data as well. Notifies listeners that nodes have changed.
        /// </summary>
        public void DebugResetGrid()
        {
            // Clear data structures
            _hexNodes.Clear();
            _neighborsCache.Clear();

            // Regenerate grid and neighbors
            GenerateGrid();
            PrecomputeNeighbors();
            // Notify listeners that nodes have updated
            foreach (var node in _hexNodes.Values)
            {
                OnNodeChanged?.Invoke(node);
            }

            Debug.Log("Hex Grid has been reset.");
        }

        /// <summary>
        /// Put this node in the dictionary for its level (create list if needed).
        /// </summary>
        private void AddNodeToLevelDictionary(HexNode node)
        {
            int level = node.NodeLevel;
            if (!_nodesByLevel.TryGetValue(level, out var nodeList))
            {
                nodeList = new List<HexNode>();
                _nodesByLevel[level] = nodeList;
            }
            nodeList.Add(node);
        }

        /// <summary>
        /// Checks whether the given position <paramref name="pos"/> is at the boundary of the hex grid.
        /// The boundary is defined by coordinates whose maximum absolute value equals the grid radius.
        /// </summary>
        /// <param name="pos">The (x, y, z) coordinates to check.</param>
        /// <returns>True if <paramref name="pos"/> is at the boundary; otherwise, false.</returns>
        private bool IsAtBoundary(Vector3Int pos)
        {
            int dist = Mathf.Max(Mathf.Abs(pos.x), Mathf.Abs(pos.y), Mathf.Abs(pos.z));
            return dist == _gridRadius;
        }

        /// <summary>
        /// Demonstrates a method of generating "branching streams" from a start node.
        /// This is a sample feature that forks paths based on random direction adjustments,
        /// eventually stopping at the boundary or if a certain queue limit is reached.
        /// </summary>
        public void GenerateBranchingStreams()
        {
            // Start from the center node
            HexNode startNode = GetHexNode(0, 0, 0);

            // The first set of forks from the center in the direction of Vector3.right
            var firstForkEnds = Fork(Vector3.right, startNode.Position, 3, 3);
            var queue = new Queue<HexNode>();

            // Only continue generating if the newly created end nodes are not on the boundary
            foreach (var end in firstForkEnds)
            {
                if (!IsAtBoundary(end.Position))
                    queue.Enqueue(end);
            }

            // Continue to fork paths from each end node, marking those end nodes along the way
            while (queue.Count > 0 && queue.Count < 100)
            {
                var end = queue.Dequeue();
                end.NodeDataInstance.NodeType = NodeType.Bonfire;  // For example, label each "end" node as a bonfire
                var direction = DirectionVector(end.Position, startNode.Position);

                // Randomly generate a number of forks from this node
                var nextForkEnds = Fork(direction, end.Position, 4, RandomNumberOfFork(end.NodeLevel, 2));

                // Add newly generated ends to the queue if they're not boundary nodes
                foreach (var nextEnd in nextForkEnds)
                {
                    if (!IsAtBoundary(nextEnd.Position))
                        queue.Enqueue(nextEnd);
                }
            }
        }

        /// <summary>
        /// Determines how many forks to generate based on the node level and a maximum possible forks.
        /// </summary>
        /// <param name="level">The level of the node from which we're forking.</param>
        /// <param name="max">The maximum number of forks.</param>
        /// <returns>Either 1 or 2, based on a probability function.</returns>
        private int RandomNumberOfFork(int level, int max)
        {
            float v = 1f / (level + 1);
            float p = v * v;
            return UnityEngine.Random.value < p ? 2 : 1;
        }

        /// <summary>
        /// Creates a direction vector from <paramref name="end"/> to <paramref name="start"/> 
        /// (essentially the difference between these two positions).
        /// </summary>
        /// <param name="end">The end node position.</param>
        /// <param name="start">The start node position.</param>
        /// <returns>A 3D direction vector.</returns>
        private Vector3 DirectionVector(Vector3Int end, Vector3Int start)
        {
            return new Vector3(end.x - start.x, end.y - start.y, end.z - start.z);
        }

        /// <summary>
        /// Creates a number of "forked" paths from a given direction and start point, each at a certain distance.
        /// The path is determined by rotating the given direction by a random angle, converting to cube coordinates,
        /// then attempting a path from the start node to the resulting end node.
        /// </summary>
        /// <param name="direction">An approximate cube-coordinate direction vector.</param>
        /// <param name="start">The starting node coordinates.</param>
        /// <param name="distance">How far from the start to place the end node.</param>
        /// <param name="numberOfForks">Number of forks to create.</param>
        /// <returns>A list of <see cref="HexNode"/> objects representing the end points of each forked path.</returns>
        private List<HexNode> Fork(Vector3 direction, Vector3Int start, int distance, int numberOfForks)
        {
            var ends = new List<HexNode>();
            HexNode startNode = GetHexNode(start.x, start.y, start.z);
            if (startNode == null) return ends;

            for (int i = 0; i < numberOfForks; i++)
            {
                // Random angle offset in the 2D plane (treating x,z as horizontal).
                bool randomBool = UnityEngine.Random.value > 0.5f;
                float angleDeg = randomBool 
                    ? UnityEngine.Random.Range(-90f, -60f) 
                    : UnityEngine.Random.Range(60f, 90f);

                // Normalize direction in the x-z plane
                Vector3 dirNorm = direction.normalized;
                float q = dirNorm.x;
                float r = dirNorm.z;

                // Rotate (q, r) in 2D by angleDeg
                Vector2 rotated = Rotate2D(new Vector2(q, r), angleDeg * Mathf.Deg2Rad);

                // Scale the rotated vector to the given distance
                Vector2 final2D = rotated.normalized * distance;
                float fx = final2D.x;
                float fz = final2D.y;
                float fy = -fx - fz;

                // Compute the actual end position in cube coordinates
                float finalX = start.x + fx;
                float finalY = start.y + fy;
                float finalZ = start.z + fz;

                // Round float coords to the nearest valid cube cell
                Vector3Int endCube = CubeRound(finalX, finalY, finalZ);

                // Attempt to retrieve the end node and find a path
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
        /// Rotates a 2D vector by the specified angle in radians.
        /// </summary>
        /// <param name="v">The original vector.</param>
        /// <param name="angleRad">The angle to rotate by, in radians.</param>
        /// <returns>The rotated vector.</returns>
        private Vector2 Rotate2D(Vector2 v, float angleRad)
        {
            float cos = Mathf.Cos(angleRad);
            float sin = Mathf.Sin(angleRad);
            float nx = v.x * cos - v.y * sin;
            float ny = v.x * sin + v.y * cos;
            return new Vector2(nx, ny);
        }

        /// <summary>
        /// Converts floating-point cube coordinates (x, y, z) to the nearest integer cube coordinates.
        /// Ensures they remain valid cube coordinates in which x + y + z = 0.
        /// Based on redblobgames.com hexagonal grid reference.
        /// </summary>
        /// <param name="x">Floating x value.</param>
        /// <param name="y">Floating y value.</param>
        /// <param name="z">Floating z value.</param>
        /// <returns>A <see cref="Vector3Int"/> representing the nearest valid cube cell.</returns>
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
        /// Finds which of the six main hex directions is closest to the given 3D <paramref name="direction"/>.
        /// This is useful for restricting movement to the canonical hex directions.
        /// </summary>
        /// <param name="direction">The direction vector to evaluate.</param>
        /// <returns>An index from 0 to 5, indicating which of the six directions is closest.</returns>
        private int FindClosestDirectionIndex(Vector3 direction)
        {
            int closestIndex = 0;
            float closestAngle = float.MaxValue;

            // The canonical hex directions in cube coordinates
            Vector3[] directionVectors = {
                new Vector3(1, -1, 0),
                new Vector3(1, 0, -1),
                new Vector3(0, 1, -1),
                new Vector3(-1, 1, 0),
                new Vector3(-1, 0, 1),
                new Vector3(0, -1, 1)
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

        /// <summary>
        /// The six main directions for hex movement in cube coordinates.
        /// Each entry corresponds to one side of the hex.
        /// </summary>
        private static readonly Vector3Int[] HexDirections = new Vector3Int[]
        {
            new Vector3Int(+1, -1, 0),
            new Vector3Int(+1, 0, -1),
            new Vector3Int(0, +1, -1),
            new Vector3Int(-1, +1, 0),
            new Vector3Int(-1, 0, +1),
            new Vector3Int(0, -1, +1)
        };

        /// <summary>
        /// Generates or finds a node at the specified <paramref name="level"/> in the grid.
        /// Returns the first node found with that level, or null if none is found.
        /// (Exact usage depends on game design.)
        /// </summary>
        /// <param name="level">The desired node level.</param>
        /// <returns>The first found node with the given level, or null.</returns>
        public HexNode GenerateNodeAtLevel(int level)
        {
            int maxLevel = level < _gridRadius ? level : _gridRadius;
            if (_nodesByLevel.TryGetValue(level, out var nodeList))
            {
                return nodeList[Random.Range(0, nodeList.Count)];
            }
            else
            {
                throw new Exception("No nodes found at level " + level);
            }
        }

        /// <summary>
        /// Assigns a generated node type (via <see cref="GenerateHexType"/>) to every node in <paramref name="path"/>,
        /// then notifies listeners that each node has changed.
        /// </summary>
        /// <param name="path">A list of nodes to assign new types to.</param>
        private void SetPathNodes(List<HexNode> path)
        {
            foreach (var node in path)
            {
                node.NodeDataInstance.NodeType = GenerateHexType();
                NotifyNodeChanged(node);
            }
        }

        /// <summary>
        /// Creates and stores a new <see cref="HexNode"/> for every valid (x, y, z) coordinate in the
        /// range determined by <see cref="_gridRadius"/>. The sum of x, y, z for a valid cell must be 0.
        /// </summary>
        private void GenerateGrid()
        {
            _hexNodes.Clear();
            for (int x = -_gridRadius; x <= _gridRadius; x++)
            {
                int minY = Mathf.Max(-_gridRadius, -x - _gridRadius);
                int maxY = Mathf.Min(_gridRadius, -x + _gridRadius);
                for (int y = minY; y <= maxY; y++)
                {
                    int z = -x - y;
                    var newNode = new HexNode(new Vector3Int(x, y, z));
                    _hexNodes.Add((x, y, z), newNode);
                    AddNodeToLevelDictionary(newNode);
                }
            }
        }

        /// <summary>
        /// Generates the <see cref="NodeData"/> object for a node, given the <paramref name="nodeType"/>.
        /// (Exact usage depends on your factory implementation.)
        /// </summary>
        /// <param name="nodeType">The node's type (e.g., forest, mountain, resource).</param>
        /// <param name="rank"></param>
        /// <returns>A new <see cref="NodeData"/> instance associated with that type.</returns>
        public NodeDataInstance GenerateNodeData(NodeType nodeType, PlayerRankEnum rank)
        {
            return MapNodeFactory.Instance.CreateNode(nodeType, rank,0);
        }

        /// <summary>
        /// Precomputes the neighbors of every node in <see cref="_hexNodes"/> and stores them in <see cref="_neighborsCache"/>.
        /// This allows for quick neighbor lookups (useful for pathfinding).
        /// </summary>
        private void PrecomputeNeighbors()
        {
            foreach (var kvp in _hexNodes)
            {
                var key = kvp.Key;
                var neighbors = new List<HexNode>(6);

                // Uses the static directions array in HexNode to check each neighbor.
                foreach (var dir in HexNode.directions)
                {
                    int nx = key.x + dir.x;
                    int ny = key.y + dir.y;
                    int nz = key.z + dir.z;
                    if (_hexNodes.TryGetValue((nx, ny, nz), out var neighborNode))
                    {
                        neighbors.Add(neighborNode);
                    }
                }

                _neighborsCache[key] = neighbors;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="HexNode"/> at position (x, y, z), or null if it doesn't exist.
        /// </summary>
        /// <param name="x">X coordinate of the node.</param>
        /// <param name="y">Y coordinate of the node.</param>
        /// <param name="z">Z coordinate of the node.</param>
        /// <returns>The corresponding <see cref="HexNode"/>, or null.</returns>
        public HexNode GetHexNode(int x, int y, int z)
        {
            _hexNodes.TryGetValue((x, y, z), out HexNode hexNode);
            return hexNode;
        }
        
        public HexNode GetHexNode(Vector3Int pos)
        {
            return GetHexNode(pos.x, pos.y, pos.z);
        }

        /// <summary>
        /// Retrieves all <see cref="HexNode"/> objects in the grid.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of all hex nodes.</returns>
        public IEnumerable<HexNode> GetAllHexNodes()
        {
            return _hexNodes.Values;
        }

        /// <summary>
        /// Retrieves all <see cref="HexNode"/> objects that are not in the <see cref="NodeExplorationState.Unrevealed"/> state.
        /// </summary>
        /// <returns>An <see cref="IEnumerable{T}"/> of all visible hex nodes.</returns>
        public IEnumerable<HexNode> GetAllVisibleHexNodes()
        {
            foreach (var node in _hexNodes.Values)
            {
                if (node.ExplorationState != NodeExplorationState.Unrevealed)
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Returns a list of nodes within the "view radius" around the provided <paramref name="playerNode"/>.
        /// </summary>
        /// <param name="playerNode">The node occupied by the player.</param>
        /// <returns>A list of nodes within the view radius.</returns>
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

                    if (_hexNodes.TryGetValue((x, y, z), out var hexNode))
                    {
                        nodesInView.Add(hexNode);
                    }
                }
            }

            return nodesInView;
        }

        /// <summary>
        /// A custom comparer for <see cref="HexNode"/> objects, used by the pathfinding open set.
        /// It sorts primarily by fCost and then by hCost as a tiebreaker.
        /// </summary>
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

        /// <summary>
        /// Finds a path from <paramref name="startNode"/> to <paramref name="goalNode"/> using 
        /// a basic A* pathfinding approach.
        /// </summary>
        /// <param name="startNode">The node from which to start.</param>
        /// <param name="goalNode">The node we're trying to reach.</param>
        /// <returns>A list of <see cref="HexNode"/> from start to goal, or null if no path is found.</returns>
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
                HexNode currentNode = openSet.Min; // Node with smallest cost
                openSet.Remove(currentNode);

                if (currentNode == goalNode)
                {
                    return RetracePath(startNode, currentNode);
                }

                closedSet.Add(currentNode);

                if (!_neighborsCache.TryGetValue((currentNode.Position.x, currentNode.Position.y, currentNode.Position.z), 
                                                 out var neighbors))
                    continue;

                foreach (HexNode neighbor in neighbors)
                {
                    // If neighbor is blocked or already evaluated, skip
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

        /// <summary>
        /// Reconstructs the path from <paramref name="endNode"/> back to <paramref name="startNode"/> 
        /// by following each node's <see cref="HexNode.Parent"/>.
        /// </summary>
        /// <param name="startNode">The node from which pathfinding began.</param>
        /// <param name="endNode">The node that was reached.</param>
        /// <returns>A list of nodes representing the path in correct order from start to end.</returns>
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

        /// <summary>
        /// Resets the pathfinding costs and parent references on all nodes before a new pathfinding run.
        /// </summary>
        private void ResetPathfindingData()
        {
            foreach (var node in _hexNodes.Values)
            {
                node.gCost = int.MaxValue;
                node.hCost = 0;
                node.Parent = null;
            }
        }

        /// <summary>
        /// Checks whether <paramref name="node"/> is adjacent to the player's current position in the grid.
        /// </summary>
        /// <param name="node">The node to check adjacency for.</param>
        /// <returns>True if <paramref name="node"/> is a neighbor of the player's node; false otherwise.</returns>
        public bool IsAdjacentToPlayer(HexNode node)
        {
            var playerNode = GetHexNode(_playerPosition.x, _playerPosition.y, _playerPosition.z);
            if (playerNode == null)
            {
                Debug.LogError("Player node not found in the grid.");
                return false;
            }

            if (_neighborsCache.TryGetValue((_playerPosition.x, _playerPosition.y, _playerPosition.z), 
                                            out var playerNeighbors))
            {
                return playerNeighbors.Contains(node);
            }

            return false;
        }

        /// <summary>
        /// Moves the player to the specified <paramref name="node"/> in the grid
        /// by setting <see cref="_playerPosition"/> to that node's coordinates.
        /// </summary>
        /// <param name="node">The node where the player should move.</param>
        public void MovePlayer(HexNode node)
        {
            _playerPosition = node.Position;
        }

        /// <summary>
        /// Finds a valid node for spawning the player. It tries the center (0, 0, 0) first,
        /// then expands outward until it finds a non-obstacle node.
        /// </summary>
        /// <returns>The coordinates of the chosen spawn node.</returns>
        public Vector3Int GenerateSpawnPoint()
        {
            var startNode = GetHexNode(0, 0, 0);
            
            // If the start node is valid, use it
            if (startNode != null && startNode.NodeDataInstance.NodeType != NodeType.Obstacle)
            {
                _playerPosition = startNode.Position;
                return _playerPosition;
            }

            // Otherwise, search outward from the center
            int searchRadius = _gridRadius;
            for (int r = 1; r <= searchRadius; r++)
            {
                for (int dx = -r; dx <= r; dx++)
                {
                    for (int dy = Mathf.Max(-r, -dx - r); dy <= Mathf.Min(r, -dx + r); dy++)
                    {
                        int dz = -dx - dy;
                        if (_hexNodes.TryGetValue((dx, dy, dz), out var node) && node.NodeDataInstance.NodeType != NodeType.Obstacle)
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

        /// <summary>
        /// Returns the current <see cref="_playerPosition"/>, effectively "spawning" the player in that location.
        /// </summary>
        /// <returns>The player's spawn position.</returns>
        public Vector3Int SpawnPlayer()
        {
            return _playerPosition;
        }

        /// <summary>
        /// Sets a single node at (x, y, z) to <see cref="NodeExplorationState.Revealed"/> 
        /// if it was previously unrevealed.
        /// </summary>
        /// <param name="x">X coordinate of the node.</param>
        /// <param name="y">Y coordinate of the node.</param>
        /// <param name="z">Z coordinate of the node.</param>
        public void RevealHexNode(int x, int y, int z)
        {
            var node = GetHexNode(x, y, z);
            if (node != null && node.ExplorationState == NodeExplorationState.Unrevealed)
            {
                node.SetExplorationState(NodeExplorationState.Revealed);
            }
        }

        /// <summary>
        /// Reveals all nodes in a given radius around (x, y, z). This changes 
        /// their <see cref="NodeExplorationState"/> to <see cref="NodeExplorationState.Revealed"/>, if unrevealed.
        /// </summary>
        /// <param name="x">X coordinate of the center.</param>
        /// <param name="y">Y coordinate of the center.</param>
        /// <param name="z">Z coordinate of the center.</param>
        /// <param name="radius">How far from (x, y, z) to reveal.</param>
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
                        NotifyNodeChanged(node);
                    }
                }
            }
        }


        #region Inverse Distance Weighting  

        private float CubeDistance(Vector3Int a, Vector3Int b)
        {
            return (Mathf.Abs(a.x - b.x) 
                    + Mathf.Abs(a.y - b.y) 
                    + Mathf.Abs(a.z - b.z)) / 2f;
        }
        
        private Vector3 CubeToWorld(Vector3Int cubeCoord)
        {
            // TODO：把 Hex 坐标转换到 3D 世界坐标的逻辑
            // 简单示例(伪)：(x, y, z) -> (x * cellWidth, 0, z * cellHeight)
            return new Vector3(
                cubeCoord.x * 1.0f, 
                0f, 
                cubeCoord.z * 0.866f // 例如等边三角的高
            );
        }

        private float EuclideanDistance(Vector3Int a, Vector3Int b)
        {
            Vector3 worldA = CubeToWorld(a);
            Vector3 worldB = CubeToWorld(b);
            return Vector3.Distance(worldA, worldB);
        }
        #endregion
        
    }
}