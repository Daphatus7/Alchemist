using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug; // Avoid collision with System.Diagnostics.Debug
using _Script.Map;
using _Script.Map.Volume;
using Edgar.Unity;
using Edgar.Unity.Examples;
using Edgar.Unity.Examples.Example2;
using ResourceSpawnVolume = _Script.Map.ResourceSpawnVolume.ResourceSpawnVolume;

namespace _Script.Managers
{
    public class SubGameManager : GameManagerBase<SubGameManager>
    {
        [Header("Optional: Dungeon Generation Example")]
        [SerializeField] private DungeonGeneratorGrid2D _dungeonGenerator;
        
        public event Action OnLevelGenerated;

        public Transform SpawnPoint => SpawnerPoint.Instance?.transform;

        public override void LoadNextLevel()
        {
            ShowLoadingScreen("SubGameManager", "Generating level...");

            if (_dungeonGenerator == null)
            {
                Debug.LogWarning("No DungeonGenerator assigned to this SubGameManager!");
                HideLoadingScreen();
                return;
            }

            // Generate the dungeon/level
            StartCoroutine(GenerateLevelCoroutine());
        }

        protected override void SingletonAwake()
        {
            // If you need something in Awake
        }

        private IEnumerator GenerateLevelCoroutine()
        {
            // Ensure this SubGameManager's scene is the active scene
            Scene myScene = gameObject.scene;
            SceneManager.SetActiveScene(myScene);

            // Wait one frame so the scene switch can fully register
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
            
            // Once generation is done, calculate reachable area
            _reachableArea = GenerateReachableArea();
            
            //ToDo: make this work spawn from spawner volume
            
            OnLevelGenerated?.Invoke();
        }

        private ReachableArea _reachableArea;
        
        public ReachableArea GenerateReachableArea()
        {
            // 1. Find the "Tilemaps" child under this GameObject
            GameObject tilemaps = null;
            for (var childIndex = 0; childIndex < transform.childCount; childIndex++)
            {
                var child = transform.GetChild(childIndex);
                if (child.name == "Tilemaps")
                {
                    tilemaps = child.gameObject;
                    break;
                }
            }

            if (!tilemaps)
            {
                Debug.LogError("No child named 'Tilemaps' found under this GameObject.");
                return null;
            }

            // 2. Get references to specific Tilemaps
            var baseTileMap = tilemaps.transform.Find("Floor").GetComponent<Tilemap>();
            var wallTileMap = tilemaps.transform.Find("Walls").GetComponent<Tilemap>();
            var colliderTileMap = tilemaps.transform.Find("Collideable").GetComponent<Tilemap>();

            if (!baseTileMap || !wallTileMap || !colliderTileMap)
            {
                throw new Exception("One or more required Tilemaps are null in ReachableArea constructor!");
            }

            // 3. Build a ReachableArea object to find the largest region
            var largestArea = new ReachableArea(baseTileMap, wallTileMap, colliderTileMap);

            // 4. Visual debug: draw lines around each tile in the largest reachable area
            VisualDebugReachableArea(baseTileMap, largestArea.reachableArea, Color.red);
            
            Debug.Log(
                $"Largest reachable area size: {largestArea.AreaSize}, " +
                $"Width: {largestArea.Width}, Height: {largestArea.Height}");
            return largestArea;
        }
        
        private void VisualDebugReachableArea(Tilemap tilemap, List<Vector2Int> reachableArea, Color debugColor)
        {
            // Draw simple squares in the Scene view around each reachable tile
            foreach (var position in reachableArea)
            {
                // Convert local tile position (0-based) back to tilemap-space coordinates
                Vector3Int tilePosition = new Vector3Int(
                    position.x + tilemap.cellBounds.xMin,
                    position.y + tilemap.cellBounds.yMin,
                    0
                );
                
                // Get the tile center in world space
                Vector3 worldPosition = tilemap.GetCellCenterWorld(tilePosition);

                // Draw lines forming a square
                const float halfSize = 0.5f;
                Vector3 bottomLeft  = worldPosition + new Vector3(-halfSize, -halfSize, 0);
                Vector3 bottomRight = worldPosition + new Vector3( halfSize, -halfSize, 0);
                Vector3 topRight    = worldPosition + new Vector3( halfSize,  halfSize, 0);
                Vector3 topLeft     = worldPosition + new Vector3(-halfSize,  halfSize, 0);

                Debug.DrawLine(bottomLeft, bottomRight, debugColor, 100f);
                Debug.DrawLine(bottomRight, topRight,   debugColor, 100f);
                Debug.DrawLine(topRight, topLeft,       debugColor, 100f);
                Debug.DrawLine(topLeft, bottomLeft,     debugColor, 100f);
            }
        }
    }
}