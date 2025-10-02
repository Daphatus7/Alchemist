using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using _Script.Character;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using Debug = UnityEngine.Debug; // Avoid collision with System.Diagnostics.Debug
using _Script.Map;
using _Script.Map.MapLoadContext.ContextInstance;
using Edgar.Unity;
using Edgar.Unity.Examples;
using Sirenix.OdinInspector;
using _Script.Map.MapExit;

namespace _Script.Managers
{
    
    public class SubGameManager : GameManagerBase<SubGameManager>
    {
        [Header("Optional: Dungeon Generation Example")]
        [SerializeField] private DungeonGeneratorGrid2D _dungeonGenerator;
        
        public Transform SpawnPoint
        {
            get
            {
                var spawnerPoint = SpawnerPoint.Instance;
                Debug.Log("SpawnerPoint found at: " + spawnerPoint);
                if (spawnerPoint)
                {
                    transform.SetParent(transform.root);
                    return spawnerPoint.GetSpawnPoint();
                }
                Debug.LogWarning("No SpawnerPoint found! Using this GameObject's transform instead.");
                return transform;
            }
        }

        
        /// <summary>
        /// Load randomly generated content for the level.
        /// </summary>
        /// <param name="instance"></param>
        /// <returns>if there is content to be generated procedurally</returns>
        /// <exception cref="Exception"></exception>
        [Button]
        public bool LoadLevelContent(MapLoadContextInstance instance)
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
                MovePlayerToScene(SpawnPoint.position, instance.MapName);
                GateGroup.Instance.GenerateGates();
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
            ApplyRuntimeTilemapCollisions();

            var spawner = GetComponent<MapSpawner>();
            GenerateNavMesh();
            spawner.Spawn(_reachableArea, instance);
            MovePlayerToScene(SpawnPoint.position, instance.MapName);
            GateGroup.Instance.GenerateGates();
        }

        private void ApplyRuntimeTilemapCollisions()
        {
            Debug.Log("Applying runtime tilemap collisions...");
            var tilemapsRoot = FindTilemapsRoot();
            if (tilemapsRoot == null)
            {
                Debug.LogWarning("Tilemaps root not found after dungeon generation. Collisions skipped.");
                return;
            }
            Debug.Log(tilemapsRoot + " is the tilemaps root");
            var destinationTilemaps = tilemapsRoot.GetComponentsInChildren<Tilemap>(true);
            if (destinationTilemaps.Length == 0)
            {
                Debug.LogWarning("No Tilemap components found under the Tilemaps root. Collisions skipped.");
                return;
            }
            
            Debug.Log($"Found {destinationTilemaps.Length} tilemaps under the Tilemaps root.");

            foreach (var layerName in new[] {"Walls", "Collideable"})
            {
                var tilemap = FindTilemapByLayerName(destinationTilemaps, layerName);
                if (tilemap == null)
                {
                    Debug.LogWarning($"Tilemap layer '{layerName}' not located after generation.");
                    continue;
                }

                EnsureTilemapCollider(tilemap.gameObject);
            }
        }
        
        [SerializeField] private Transform tilemapsRootOverride;
        
        private Transform FindTilemapsRoot()
        {
            // return self
            // the tilemaps root is a child named "Tilemaps" so get that child
            var child = transform.Find("Tilemaps");
            if (child != null)
            {
                return child;
            }            
            return null;
        }

        private static Transform FindChildRecursive(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (string.Equals(child.name, childName, StringComparison.OrdinalIgnoreCase))
                {
                    return child;
                }

                var descendant = FindChildRecursive(child, childName);
                if (descendant != null)
                {
                    return descendant;
                }
            }

            return null;
        }

        private static Tilemap FindTilemapByLayerName(IEnumerable<Tilemap> tilemaps, string targetName)
        {
            foreach (var tilemap in tilemaps)
            {
                var gameObjectName = tilemap.gameObject.name;
                if (string.Equals(gameObjectName, targetName, StringComparison.OrdinalIgnoreCase) ||
                    gameObjectName.IndexOf(targetName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return tilemap;
                }
            }

            return null;
        }

        private void EnsureTilemapCollider(GameObject tilemapGameObject)
        {
            var tilemap = tilemapGameObject.GetComponent<Tilemap>();
            if (tilemap == null)
            {
                Debug.LogWarning($"GameObject '{tilemapGameObject.name}' lacks a Tilemap component.");
                return;
            }

            var tilemapCollider = tilemapGameObject.GetComponent<TilemapCollider2D>();
            if (tilemapCollider == null)
            {
                tilemapCollider = tilemapGameObject.AddComponent<TilemapCollider2D>();
            }

            tilemapCollider.usedByComposite = true;
            tilemapCollider.ProcessTilemapChanges();

            var compositeCollider = tilemapGameObject.GetComponent<CompositeCollider2D>();
            if (compositeCollider == null)
            {
                compositeCollider = tilemapGameObject.AddComponent<CompositeCollider2D>();
            }

            compositeCollider.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider.generationType = CompositeCollider2D.GenerationType.Synchronous;
            compositeCollider.isTrigger = false;

            var rigidbody = tilemapGameObject.GetComponent<Rigidbody2D>();
            if (rigidbody == null)
            {
                rigidbody = tilemapGameObject.AddComponent<Rigidbody2D>();
            }

            rigidbody.bodyType = RigidbodyType2D.Static;
            rigidbody.simulated = true;

            var obstacleLayer = LayerMask.NameToLayer("Obstacle");
            if (obstacleLayer != -1)
            {
                tilemapGameObject.layer = obstacleLayer;
            }
        }

        private void GenerateNavMesh()
        {
            var gridGraph = AstarPath.active.data.gridGraph;

            if (gridGraph != null)
            {
                // Set the size of the graph
                gridGraph.center = Instance.MapCenter;
                gridGraph.width = Instance.MapBounds.x * 2;
                gridGraph.depth = Instance.MapBounds.y * 2;
                gridGraph.nodeSize = 0.5f;        // Size of each node in world units
                // Optionally adjust boundaries
                gridGraph.UpdateSizeFromWidthDepth();
            }
            GameManager.Instance.AstarPath.Scan();
        }
        private void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {

            if (GameManager.Instance.PlayerCharacter is { } playerCharacter)
            {
                playerCharacter.transform.position = spawnPosition;
                Debug.Log($"Player moved to {spawnPosition} in scene {targetScene}.");
            }
            else
            {
                Debug.Log($"character is null");
            }
        }

        #region Reachable Area Generation
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
        #endregion
    }
}
