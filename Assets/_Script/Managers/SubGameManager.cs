using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug; // Avoid collision with System.Diagnostics.Debug
using _Script.Map;
using _Script.Map.MapLoadContext;
using _Script.Map.MapLoadContext.ContextInstance;
using Edgar.Unity;
using Edgar.Unity.Examples;
using Sirenix.OdinInspector;

namespace _Script.Managers
{
    public class SubGameManager : GameManagerBase<SubGameManager>
    {
        [Header("Optional: Dungeon Generation Example")]
        [SerializeField] private DungeonGeneratorGrid2D _dungeonGenerator;
        
        public event Action OnLevelGenerated;

        public Transform SpawnPoint
        {
            get
            {
                var spawnerPoint = SpawnerPoint.Instance;
                if (spawnerPoint)
                {
                    transform.SetParent(transform.root);
                    return spawnerPoint.GetSpawnPoint();
                }
                Debug.LogWarning("No SpawnerPoint found! Using this GameObject's transform instead.");
                return transform;
            }
        }

        [Button]
        public new bool LoadNextLevel(MapLoadContextInstance instance)
        {
            ShowLoadingScreen("SubGameManager", "Generating level...");

            if (!_dungeonGenerator)
            {
                Debug.LogWarning("No DungeonGenerator assigned to this SubGameManager!");
                HideLoadingScreen();
                var spawner = GetComponent<MapSpawner>();
                if (!spawner)
                {
                    throw new Exception("dont have a generator but still trying to access the spawner");
                }
                spawner.Spawn(instance);
                return false;
            }
            
            // Generate the dungeon/level
            StartCoroutine(GenerateLevelCoroutine(instance));
            return true;
        }

        protected override void SingletonAwake()
        {
            // If you need something in Awake
        }

        public override bool LoadNextLevel()
        {
            throw new NotImplementedException();
        }

        private IEnumerator GenerateLevelCoroutine(MapLoadContextInstance instance)
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

            var spawner = GetComponent<MapSpawner>();
            spawner.Spawn(_reachableArea, instance);
            OnLevelGenerated?.Invoke();
        }

        private ReachableArea _reachableArea; public ReachableArea ReachableArea => _reachableArea;
        private Tilemap _baseTileMap;
        public Vector3 MapCenter => _reachableArea.Pivot;
        
        public Vector2Int MapBounds => new Vector2Int(_reachableArea.Width, _reachableArea.Height);

        private ReachableArea GenerateReachableArea()
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
                throw new Exception("No 'Tilemaps' child found under SubGameManager!");
            }

            // 2. Get references to specific Tilemaps
            _baseTileMap = tilemaps.transform.Find("Floor").GetComponent<Tilemap>();
            var wallTile = tilemaps.transform.Find("Walls");
            
            wallTile.gameObject.layer = LayerMask.NameToLayer("Obstacle");
            if (wallTile == null)
            {
                throw new Exception("No 'Walls' Tilemap found under SubGameManager!");
            }

            var wallTileMap = wallTile.GetComponent<Tilemap>();

            var colliderTile = tilemaps.transform.Find("Collideable"); 
            if (colliderTile == null)
            {
                throw new Exception("No 'Collideable' Tilemap found under SubGameManager!");
            }
            
            colliderTile.gameObject.layer = LayerMask.NameToLayer("Obstacle");

            var colliderTileMap = colliderTile.GetComponent<Tilemap>();
            if (colliderTileMap == null)
            {
                throw new Exception("No 'Collideable' Tilemap found under SubGameManager!");
            }

            // 3. Build a ReachableArea object to find the largest region
            var largestArea = new ReachableArea(_baseTileMap, wallTileMap, colliderTileMap);

            // 4. Visual debug: draw lines around each tile in the largest reachable area
            VisualDebugReachableArea(_baseTileMap, largestArea.reachableArea, Color.red);
            
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