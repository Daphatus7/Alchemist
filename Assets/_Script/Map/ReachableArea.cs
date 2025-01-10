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
        // The largest contiguous walkable region, stored as local indices [0..Width-1, 0..Height-1].
        // "Local indices" means relative to the tilemap's bounding box.
        public List<Vector2Int> reachableArea { get; }

        private readonly Vector3 _pivot; public Vector3 Pivot => _pivot;
        
        public Vector3 GetARandomPosition()
        {
            if (reachableArea.Count == 0)
            {
                Debug.LogError("No reachable area found!");
                return Vector3.zero;
            }

            var randomIndex = Random.Range(0, reachableArea.Count);
            return LocalToTilemapCoords(reachableArea[randomIndex]);
        }
        
        // Overall dimensions derived from the Floor tilemap's bounding box
        public int Width { get; }

        public int Height { get; }

        // The count of tiles in the largest region
        public int AreaSize { get; }

        // 2D array marking walkable (true) vs. blocked (false) for each cell
        private readonly bool[,] _walkableArea;
        
        private Tilemap baseTileMap;
        
        /// <summary>
        /// Constructs a ReachableArea by scanning the provided Tilemaps
        /// and finding the largest walkable region.
        /// </summary>
        public ReachableArea(Tilemap floorTileMap, Tilemap wallsTileMap, Tilemap colliderTileMap)
        {
            if (floorTileMap == null || wallsTileMap == null || colliderTileMap == null)
            {
                Debug.LogError("One or more required Tilemaps are null in ReachableArea constructor!");
                reachableArea = new List<Vector2Int>();
                return;
            }
            
            // 1. Determine the bounding region from the Floor tilemap
            BoundsInt bounds = floorTileMap.cellBounds;

            Width = bounds.size.x;
            Height = bounds.size.y;
            _pivot = floorTileMap.GetCellCenterWorld(new Vector3Int(
                bounds.xMin + bounds.size.x / 2,
                bounds.yMin + bounds.size.y / 2,
                0
            ));
            baseTileMap = floorTileMap;
            // 2. Allocate and fill the _walkableArea array
            _walkableArea = new bool[Width, Height];
            FillWalkableArea(floorTileMap, wallsTileMap, colliderTileMap, bounds);

            // 3. Run BFS to find the single largest reachable region
            reachableArea = FindLargestReachableAreaBFS();
            AreaSize = reachableArea.Count;
        }

        /// <summary>
        /// Marks each cell as walkable if:
        ///     - There's a tile in Floor
        ///     - AND no tile in Walls or Collideable
        /// Then stores the result in _walkableArea.
        /// 
        /// localX, localY are in [0..Width-1, 0..Height-1], so we do:
        ///   localX = x - bounds.xMin,
        ///   localY = y - bounds.yMin.
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
        /// The BFS works on local coordinates [0.._width-1, 0.._height-1].
        /// </summary>
        private List<Vector2Int> FindLargestReachableAreaBFS()
        {
            bool[,] visited = new bool[Width, Height];
            List<Vector2Int> largestArea = new List<Vector2Int>();
            int maxCount = 0;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    // If this cell is walkable and not visited, we BFS from here
                    if (_walkableArea[x, y] && !visited[x, y])
                    {
                        List<Vector2Int> currentArea = new List<Vector2Int>();
                        Queue<Vector2Int> queue = new Queue<Vector2Int>();
                        
                        queue.Enqueue(new Vector2Int(x, y));
                        visited[x, y] = true;

                        while (queue.Count > 0)
                        {
                            Vector2Int cell = queue.Dequeue();
                            currentArea.Add(cell);

                            // Check the 4 orthogonal neighbors
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

                                // In-bounds check
                                if (nx >= 0 && nx < Width &&
                                    ny >= 0 && ny < Height)
                                {
                                    if (_walkableArea[nx, ny] && !visited[nx, ny])
                                    {
                                        visited[nx, ny] = true;
                                        queue.Enqueue(new Vector2Int(nx, ny));
                                    }
                                }
                            }
                        }

                        // Compare this region's size with the largest found so far
                        if (currentArea.Count > maxCount)
                        {
                            maxCount = currentArea.Count;
                            largestArea = currentArea;
                        }
                    }
                }
            }

            Debug.Log($"Largest reachable area tile count = {maxCount}");
            return largestArea;
        }

        /*
         * Example helper (if you need it):
         * Convert a local BFS coordinate to the actual tilemap cell coordinate.
         *
         *   Vector2Int localCoord  = new Vector2Int( x, y );
         *   Vector3Int tileCoord  = LocalToTilemapCoords(localCoord);
         *   Vector3   worldCenter = floorTilemap.GetCellCenterWorld(tileCoord);
         */
        private Vector3 LocalToTilemapCoords(Vector2Int localCoord)
        {
            Vector3Int tilePosition = new Vector3Int(
                localCoord.x + baseTileMap.cellBounds.xMin,
                localCoord.y + baseTileMap.cellBounds.yMin,
                0
            );
                
            // Get the tile center in world space
            Vector3 worldPosition = baseTileMap.GetCellCenterWorld(tilePosition);
            return worldPosition;
        }
    }
}