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

        private int _weightSum;
        private int _obstacleWeight;
        private int _resourceWeight;
        private int _enemyWeight;
        private int _campfireWeight;
        private int _bossWeight;
        
        public event Action<HexNode> OnNodeChanged;

        private void NotifyNodeChanged(HexNode node)
        {
            OnNodeChanged?.Invoke(node);
        }
        
        private Vector3Int _playerPosition; 
        public Vector3Int PlayerPosition => _playerPosition;

        public HexGrid(int gridRadius, GridConfiguration gridConfiguration)
        {
            _gridRadius = gridRadius;
            _gridConfiguration = gridConfiguration;
            GenerateGrid();
            PrecomputeNeighbors();
            GenerateBranchingStreams();
        }
        public void DebugResetGrid()
        {
            // 清空数据结构
            hexNodes.Clear();
            _neighborsCache.Clear();

            // 重新生成网格和邻居
            GenerateGrid();
            PrecomputeNeighbors();

            // 可根据需要重新执行分叉逻辑，或不执行
            // 如果需要再次生成分叉路径：
            GenerateBranchingStreams();

            // 通知监听者节点更新（可选）
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
    
            var firstForkEnds = Fork(Vector3.right, startNode.Position, 3, 1);
    
            var queue = new Queue<HexNode>();

            // 初始末端节点加入队列时检查是否在边界
            foreach (var end in firstForkEnds)
            {
                if (!IsAtBoundary(end.Position))
                    queue.Enqueue(end);
            }
    
            // 若没有达到边界的节点才会继续生成新路径
            while(queue.Count is > 0 and < 30)
            {
                var end = queue.Dequeue();
                var direction = DirectionVector(end.Position, startNode.Position);
                var nextForkEnds = Fork(direction, end.Position, 5, RandomNumberOfFork(end.NodeLevel, 1));
        
                // 对每个新生成的终点检查是否在边界
                foreach (var nextEnd in nextForkEnds)
                {
                    if (!IsAtBoundary(nextEnd.Position))
                        queue.Enqueue(nextEnd);
                }
            }
        }
        
        /// <summary>
        /// 使用概率分布决定分叉数量:
        /// - 候选分叉数量范围为[1, max]
        /// - 概率权重 = 1/(i+level)，i为分叉数量
        /// - 随着level增大，对大分叉数i的概率降低
        /// </summary>
        private int RandomNumberOfFork(int level, int max)
        {
            if (max <= 1) return 1;

            float[] weights = new float[max];
            float totalWeight = 0f;

            for (int i = 0; i < max; i++)
            {
                // i表示分叉数量-1（因为数组从0开始）
                int forksCount = i + 1;
                float w = 1f / (forksCount + level);
                weights[i] = w;
                totalWeight += w;
            }

            // 标准化后抽取随机数
            float randVal = UnityEngine.Random.value * totalWeight;
            float cumulative = 0f;
            for (int i = 0; i < max; i++)
            {
                cumulative += weights[i];
                if (randVal <= cumulative)
                {
                    return i + 1; // i+1即实际分叉数
                }
            }

            // 理论上不会走到这里，如果走到这里，返回max作为保底
            return max;
        }    
        
        
        private Vector3 DirectionVector(Vector3Int end, Vector3Int start)
        {
            return new Vector3(end.x - start.x, end.y - start.y, end.z - start.z);
        }
        
        /// <summary>
        /// 从给定方向和起点生成叉路，不局限于60度方向。
        /// 给定的direction为立方坐标方向矢量(approx)。
        /// 我们在direction上增加一个小随机角度（例如±30度），再映射回立方坐标。
        /// </summary>

        private List<HexNode> Fork(Vector3 direction, Vector3Int start, int distance, int numberOfForks)
        {
            List<HexNode> ends = new List<HexNode>();
            HexNode startNode = GetHexNode(start.x, start.y, start.z);
            if (startNode == null) return ends;

            for (int i = 0; i < numberOfForks; i++)
            {
                // 在-90度到90度之间随机一个偏转角度
                
                //random bool
                bool randomBool = UnityEngine.Random.value > 0.5f;
                float angleDeg = randomBool ? UnityEngine.Random.Range(-90f, -60f) : UnityEngine.Random.Range(60f, 90f);

                // 将direction归一化，使用x,z作为2D平面
                Vector3 dirNorm = direction.normalized;
                float q = dirNorm.x;
                float r = dirNorm.z;

                // 将(q,r)在2D平面旋转angleDeg度
                Vector2 rotated = Rotate2D(new Vector2(q, r), angleDeg * Mathf.Deg2Rad);

                // 将旋转后的向量放大到distance长度
                Vector2 final2D = rotated.normalized * distance;

                // 将final2D(q,r)转回立方坐标
                float fx = final2D.x;
                float fz = final2D.y;
                float fy = -fx - fz;

                // 基于start作为基准点，计算最终节点坐标
                float finalX = start.x + fx;
                float finalY = start.y + fy;
                float finalZ = start.z + fz;

                // 对浮点坐标进行四舍五入到最近的hex格点
                Vector3Int endCube = CubeRound(finalX, finalY, finalZ);

                // 寻找endNode并尝试寻路
                HexNode endNode = GetHexNode(endCube.x, endCube.y, endCube.z);
                if (endNode != null)
                {
                    var path = FindPath(startNode, endNode);
                    if (path != null && path.Count > 0)
                    {
                        SetPathNodes(path, NodeType.Resource);
                        ends.Add(endNode);
                    }
                }
            }

            return ends;
        }
        
        /// <summary>
        /// 2D向量旋转函数，以弧度为单位
        /// angleRad: 旋转角度(弧度)
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
        /// 将浮点cube坐标四舍五入到最近的立方格点
        /// 算法参考：Red Blob Games: https://www.redblobgames.com/grids/hexagons/
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
            {
                rx = -ry - rz;
            }
            else if (dy > dz)
            {
                ry = -rx - rz;
            }
            else
            {
                rz = -rx - ry;
            }

            return new Vector3Int(rx, ry, rz);
        }

        /// <summary>
        /// 找到与给定Vector3方向最接近的基本六方向索引
        /// </summary>
        private int FindClosestDirectionIndex(Vector3 direction)
        {
            int closestIndex = 0;
            float closestAngle = float.MaxValue;

            // 与HexDirections对应的世界方向向量（根据项目需要可调整）
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
                Vector3 candidate = directionVectors[i].normalized;
                float angle = Vector3.Angle(dirNorm, candidate);
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
            new Vector3Int(+1, -1, 0),  // D0
            new Vector3Int(+1, 0, -1),  // D1
            new Vector3Int(0, +1, -1),  // D2
            new Vector3Int(-1, +1, 0),  // D3
            new Vector3Int(-1, 0, +1),  // D4
            new Vector3Int(0, -1, +1)   // D5
        };
        
        public HexNode GenerateNodeAtLevel(int level)
        {
            HexNode node = null;
            
            for (int x = -_gridRadius; x <= _gridRadius; x++)
            {
                for (int y = Mathf.Max(-_gridRadius, -x - _gridRadius); y <= Mathf.Min(_gridRadius, -x + _gridRadius); y++)
                {
                    int z = -x - y;
                    var checkNode = GetHexNode(x, y, z);
                    if (checkNode != null && checkNode.NodeLevel == level)
                    {
                        node = checkNode;
                        break;
                    }
                }
            }
            return node;
        }
        
        private void SetPathNodes(List<HexNode> path, NodeType nodeType)
        {
            foreach (var node in path)
            {
                node.NodeType = nodeType;
                Debug.Log($"Node at {node.Position} set to {nodeType}");
                NotifyNodeChanged(node);
            }
        }

        private void GenerateGrid()
        {
            hexNodes.Clear();
            for (int x = -_gridRadius; x <= _gridRadius; x++)
            {
                for (int y = Mathf.Max(-_gridRadius, -x - _gridRadius); y <= Mathf.Min(_gridRadius, -x + _gridRadius); y++)
                {
                    int z = -x - y;
                    NodeType newNodeType = NodeType.Empty;
                    HexNode hexNode = new HexNode(new Vector3Int(x, y, z), newNodeType, GenerateNodeData(newNodeType));
                    hexNodes.Add((x, y, z), hexNode);
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