using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using _Script.Map;
using Edgar.Unity;
using Edgar.Unity.Examples;
using Edgar.Unity.Examples.Example2;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug;

namespace _Script.Managers
{
    public class SubGameManager : GameManagerBase<SubGameManager>
    {
        [Header("Optional: Dungeon Generation Example")]
        [SerializeField] private DungeonGeneratorGrid2D _dungeonGenerator;
        
        public event System.Action OnLevelGenerated;

        public Transform SpawnPoint => SpawnerPoint.Instance ? .transform;

        /**
         * 加载下一个关卡
         */
        public override void LoadNextLevel()
        {
            ShowLoadingScreen("SubGameManager", "Generating level...");

            if (_dungeonGenerator == null)
            {
                Debug.LogWarning("No DungeonGenerator assigned to this SubGameManager!");
                HideLoadingScreen();
                return;
            }

            //生成关卡
            StartCoroutine(GenerateLevelCoroutine());
        }

        protected override void SingletonAwake()
        {
            // If you need something in Awake
        }

        private IEnumerator GenerateLevelCoroutine()
        {
            // Ensure this SubGameManager's scene is the active scene
            Scene myScene = gameObject.scene;    // The scene containing the SubGameManager MonoBehaviour
            
            SceneManager.SetActiveScene(myScene);

            // 2. Wait one frame so the scene switch can fully register
            yield return null;
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Wait one frame so the loading screen becomes visible
            yield return null;

            // Now generate the dungeon
            var payload = _dungeonGenerator.Generate();
            
            // Wait another frame to let newly spawned objects initialize
            yield return null;

            stopwatch.Stop();
            double seconds = stopwatch.ElapsedMilliseconds / 1000d;

            SetLevelInfo($"Generated in {seconds:F2}s");
            HideLoadingScreen();
            
            GetTileMap();

            //加载完毕
            OnLevelGenerated?.Invoke();
        }

        
        private List<List<Vector2Int>> _reachableAreas;
        private bool[,] _walkableArea;
        private int _width;
        private int _height;
        
        
        
        public void GetTileMap()
        {
            GameObject tilemaps = null;
            for(var childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                var child = transform.GetChild(childIndex);
                if(child.name == "Tilemaps")
                {
                    tilemaps = child.gameObject;
                }
            }
            
            //get the base tile map, which is where the walkable tiles are
            if (tilemaps != null)
            {
                var baseTileMap = tilemaps.transform.Find("Floor").GetComponent<Tilemap>();
                var wallTileMap = tilemaps.transform.Find("Walls").GetComponent<Tilemap>();
                var colliderTileMap = tilemaps.transform.Find("Collideable").GetComponent<Tilemap>();

                if (baseTileMap == null || wallTileMap == null || colliderTileMap == null)
                {
                    throw new Exception("Tilemaps not found!");
                }
                var largestArea =GetLargestReachableArea(baseTileMap, wallTileMap, colliderTileMap);
                
                VisualDebugReachableArea(baseTileMap, largestArea, Color.red);
                
            }
        }
        
        
        private void VisualDebugReachableArea(Tilemap tilemap, List<Vector2Int> reachableArea, Color debugColor)
        {
            foreach (var position in reachableArea)
            {
                // Convert local tile position to world position
                Vector3Int tilePosition = new Vector3Int(position.x + tilemap.cellBounds.xMin, position.y + tilemap.cellBounds.yMin, 0);
                Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);

                // Draw a square or marker to represent the reachable tile
                Debug.DrawLine(worldPosition + new Vector3(-0.5f, -0.5f, 0), worldPosition + new Vector3(0.5f, -0.5f, 0), debugColor, 100f);
                Debug.DrawLine(worldPosition + new Vector3(0.5f, -0.5f, 0), worldPosition + new Vector3(0.5f, 0.5f, 0), debugColor, 100f);
                Debug.DrawLine(worldPosition + new Vector3(0.5f, 0.5f, 0), worldPosition + new Vector3(-0.5f, 0.5f, 0), debugColor, 100f);
                Debug.DrawLine(worldPosition + new Vector3(-0.5f, 0.5f, 0), worldPosition + new Vector3(-0.5f, -0.5f, 0), debugColor, 100f);
            }
        }

        // Call this in GetTileMap after finding the largest reachable area
        
        private List<Vector2Int> GetLargestReachableArea(Tilemap floorTileMap, Tilemap wallsTileMap,Tilemap colliderTileMap)
        {
            // 1. Find the "Tilemaps" child
            GameObject tilemaps = transform.Find("Tilemaps")?.gameObject;
            if (tilemaps == null)
            {
                Debug.LogError("No child named 'Tilemaps' found under this GameObject.");
                return new List<Vector2Int>();
            }
            

            if (floorTileMap == null || wallsTileMap == null || colliderTileMap == null)
            {
                Debug.LogError("Required Tilemaps (Floor/Walls/Collideable) are missing!");
                return new List<Vector2Int>();
            }

            // 3. Determine the bounds of the Floor tilemap
            BoundsInt bounds = floorTileMap.cellBounds;
            _width  = bounds.size.x;
            _height = bounds.size.y;

            // Initialize and fill the _walkableArea array
            _walkableArea = new bool[_width, _height];
            FillWalkableArea(floorTileMap, wallsTileMap, colliderTileMap, bounds);

            // 4. Find the largest reachable area using BFS without storing all areas
            return FindLargestReachableAreaBFS();
        }

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
                        // BFS to find the extent of this region
                        List<Vector2Int> currentArea = new List<Vector2Int>();
                        Queue<Vector2Int> queue = new Queue<Vector2Int>();
                        queue.Enqueue(new Vector2Int(x, y));
                        visited[x, y] = true;

                        while (queue.Count > 0)
                        {
                            var cell = queue.Dequeue();
                            currentArea.Add(cell);

                            // Check neighbors (4-directional)
                            foreach (var dir in new Vector2Int[]
                            {
                                new Vector2Int(1, 0),
                                new Vector2Int(-1, 0),
                                new Vector2Int(0, 1),
                                new Vector2Int(0, -1),
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

                        // If this region is bigger than our current max, update largestArea
                        if (currentArea.Count > maxCount)
                        {
                            maxCount = currentArea.Count;
                            largestArea = currentArea;
                        }
                    }
                }
            }
            Debug.Log($"Largest area size = {maxCount}");
            return largestArea;
        }
    }
    
}