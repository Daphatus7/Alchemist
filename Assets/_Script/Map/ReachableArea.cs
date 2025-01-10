// Author : Peiyu Wang @ Daphatus
// 10 01 2025 01 36

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map
{
    /// <summary>
    /// Holds information about the largest reachable region in a given set of Tilemaps.
    /// </summary>
    public class ReachableArea
    {
        // The largest contiguous walkable region, stored as local indices [0..Width-1, 0..Height-1]
        private List<Vector2Int> _reachableArea; public List<Vector2Int> reachableArea => _reachableArea;

        // Overall dimensions derived from the Floor tilemap's bounding box
        private int _width; public int Width => _width;

        private int _height; public int Height => _height;
        
        // The count of tiles in the largest region
        private int _areaSize; public int AreaSize => _areaSize;
        
        // Internal 2D array marking walkable vs. blocked for each cell
        private bool[,] _walkableArea;

        /// <summary>
        /// Constructs a ReachableArea by scanning the provided Tilemaps
        /// and finding the largest walkable region.
        /// </summary>
        public ReachableArea(Tilemap floorTileMap, Tilemap wallsTileMap, Tilemap colliderTileMap)
        {
            if (floorTileMap == null || wallsTileMap == null || colliderTileMap == null)
            {
                Debug.LogError("One or more required Tilemaps are null in ReachableArea constructor!");
                _reachableArea = new List<Vector2Int>();
                return;
            }
            
            // 1. Determine the bounding region from the Floor tilemap
            BoundsInt bounds = floorTileMap.cellBounds;
            _width = bounds.size.x;
            _height = bounds.size.y;

            // 2. Allocate and fill the _walkableArea array
            _walkableArea = new bool[_width, _height];
            FillWalkableArea(floorTileMap, wallsTileMap, colliderTileMap, bounds);

            // 3. Run BFS to find the single largest reachable region
            _reachableArea = FindLargestReachableAreaBFS();
            _areaSize = _reachableArea.Count;
        }

        /// <summary>
        /// Marks each cell as walkable if:
        ///     - There's a tile in Floor
        ///     - AND no tile in Walls or Collideable
        /// Then stores the result in _walkableArea.
        /// </summary>
        private void FillWalkableArea(Tilemap floor, Tilemap walls, Tilemap coll, BoundsInt bounds)
        {
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    int localX = x - bounds.xMin;
                    int localY = y - bounds.yMin;

                    bool hasFloor = (floor.GetTile(new Vector3Int(x, y, 0)) != null);
                    bool hasWall  = (walls.GetTile(new Vector3Int(x, y, 0)) != null);
                    bool hasColl  = (coll.GetTile(new Vector3Int(x, y, 0)) != null);

                    _walkableArea[localX, localY] = hasFloor && !hasWall && !hasColl;
                }
            }
        }

        /// <summary>
        /// Finds the single largest connected component in _walkableArea
        /// using a BFS approach (4-directional).
        /// </summary>
        private List<Vector2Int> FindLargestReachableAreaBFS()
        {
            bool[,] visited = new bool[_width, _height];
            List<Vector2Int> largestArea = new List<Vector2Int>();
            int maxCount = 0;

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    if (_walkableArea[x, y] && !visited[x, y])
                    {
                        // BFS for this new region
                        List<Vector2Int> currentArea = new List<Vector2Int>();
                        Queue<Vector2Int> queue = new Queue<Vector2Int>();
                        queue.Enqueue(new Vector2Int(x, y));
                        visited[x, y] = true;

                        while (queue.Count > 0)
                        {
                            Vector2Int cell = queue.Dequeue();
                            currentArea.Add(cell);

                            // Check 4 neighbors
                            foreach (var dir in new[]
                            {
                                new Vector2Int( 1,  0),
                                new Vector2Int(-1,  0),
                                new Vector2Int( 0,  1),
                                new Vector2Int( 0, -1),
                            })
                            {
                                int nx = cell.x + dir.x;
                                int ny = cell.y + dir.y;

                                if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                                {
                                    if (_walkableArea[nx, ny] && !visited[nx, ny])
                                    {
                                        visited[nx, ny] = true;
                                        queue.Enqueue(new Vector2Int(nx, ny));
                                    }
                                }
                            }
                        }

                        // Compare with current largest
                        if (currentArea.Count > maxCount)
                        {
                            maxCount = currentArea.Count;
                            largestArea = currentArea; // store the new largest
                        }
                    }
                }
            }

            Debug.Log($"Largest reachable area tile count = {maxCount}");
            return largestArea;
        }
    }
}