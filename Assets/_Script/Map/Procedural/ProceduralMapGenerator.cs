using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Procedural
{
    public class ProceduralMapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")]
        public int width = 50;
        public int height = 50;

        [Header("Tiles")]
        public TileBase wallTile;
        public TileBase grassTile;
        public TileBase dirtTile;
        public TileBase waterTile;
        public TileBase forestTile;
        public TileBase poiTile;

        [Header("Flora Settings")]
        public Tilemap floraTilemap;
        public TileBase floraTile;
        [Range(0.0f, 1.0f)] public float floraDensity = 0.3f;
        [Range(0.0f, 1.0f)] public float floraNoiseScale = 0.05f;
        public float floraMinNoise = 0.4f;
        public float floraMaxNoise = 0.6f;

        [Header("Rocks Settings")]
        public Tilemap obstaclesTilemap;
        public TileBase rockTile;
        [Range(0.0f, 1.0f)] public float rockDensity = 0.2f;
        [Range(0.0f, 1.0f)] public float rockNoiseScale = 0.05f;
        public float rockMinNoise = 0.5f;
        public float rockMaxNoise = 0.8f;

        [Header("Tilemap Reference")]
        public Tilemap baseTilemap;

        [Header("Noise Settings")]
        [Range(0.0f, 1.0f)] public float perlinScale = 0.1f;
        [Range(0, 100)] public int forestThreshold = 50;
        [Range(0, 100)] public int waterThreshold = 35;

        [Header("POI Settings")]
        public int numberOfPOIs = 3;
        public int minDistanceBetweenPOIs = 8;

        [Header("Monster Spawn Settings")]
        public GameObject monsterSpawnPrefab;
        public int numberOfMonsterSpawns = 5;
        public float minDistanceBetweenMonsterSpawns = 10f;

        [Header("Seed")]
        public int seed = 0;
        public bool useRandomSeed = false;

        private TileBase[,] _mapTiles;
        private bool[,] _walkableArea;
        private TileBase[,] _obstacleTiles; // Stores placed obstacle tiles (e.g., rocks)

        void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            if (baseTilemap == null || floraTilemap == null || obstaclesTilemap == null)
            {
                Debug.LogError("Please assign baseTilemap, floraTilemap, and obstaclesTilemap in the inspector.");
                return;
            }

            if (!useRandomSeed)
                Random.InitState(seed);
            else
                Random.InitState(System.DateTime.Now.GetHashCode());

            _mapTiles = new TileBase[width, height];
            _walkableArea = new bool[width, height];
            _obstacleTiles = new TileBase[width, height];

            InitializeBoundary();
            GenerateWalkableArea();
            AddPerlinNoiseTerrain();
            PlacePOIs();
            PlaceFlora();
            PlaceRocks();
            RenderFinalMap();
            PlaceMonsterSpawns();
        }

        void InitializeBoundary()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        _mapTiles[x, y] = wallTile;
                        _walkableArea[x, y] = false;
                    }
                    else
                    {
                        _mapTiles[x, y] = null;
                    }
                }
            }
        }

        void GenerateWalkableArea()
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    _walkableArea[x, y] = Random.value > 0.3f;
                }
            }

            for (int i = 0; i < 3; i++)
            {
                _walkableArea = SmoothWalkableArea(_walkableArea);
            }

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (_walkableArea[x, y])
                    {
                        _mapTiles[x, y] = (Random.value > 0.5f) ? grassTile : dirtTile;
                    }
                    else
                    {
                        _mapTiles[x, y] = wallTile;
                    }
                }
            }
        }

        bool[,] SmoothWalkableArea(bool[,] area)
        {
            bool[,] newArea = new bool[width, height];
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int neighborCount = CountWalkableNeighbors(area, x, y);
                    newArea[x, y] = neighborCount > 4;
                }
            }
            return newArea;
        }

        int CountWalkableNeighbors(bool[,] area, int cx, int cy)
        {
            int count = 0;
            for (int nx = cx - 1; nx <= cx + 1; nx++)
            {
                for (int ny = cy - 1; ny <= cy + 1; ny++)
                {
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        if (area[nx, ny]) count++;
                    }
                }
            }
            return count;
        }

        void AddPerlinNoiseTerrain()
        {
            float xOffset = Random.Range(0f, 1000f);
            float yOffset = Random.Range(0f, 1000f);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (_mapTiles[x, y] != null && _mapTiles[x, y] != wallTile)
                    {
                        float noiseValue = Mathf.PerlinNoise((x + xOffset) * perlinScale, (y + yOffset) * perlinScale) * 100f;

                        if (noiseValue > forestThreshold)
                        {
                            _mapTiles[x, y] = forestTile;
                            _walkableArea[x, y] = true;  // Forest still walkable if you want, or set to false if needed
                        }
                        else if (noiseValue < waterThreshold)
                        {
                            _mapTiles[x, y] = waterTile;
                            _walkableArea[x, y] = false;
                        }
                    }
                }
            }
        }

        void PlacePOIs()
        {
            List<Vector2Int> placedPOIs = new List<Vector2Int>();

            int attempts = 0;
            int maxAttempts = numberOfPOIs * 50;

            while (placedPOIs.Count < numberOfPOIs && attempts < maxAttempts)
            {
                attempts++;
                int x = Random.Range(2, width - 2);
                int y = Random.Range(2, height - 2);

                if (_mapTiles[x, y] == grassTile || _mapTiles[x, y] == dirtTile)
                {
                    bool tooClose = false;
                    foreach (var poi in placedPOIs)
                    {
                        if (Vector2Int.Distance(poi, new Vector2Int(x, y)) < minDistanceBetweenPOIs)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        _mapTiles[x, y] = poiTile;
                        placedPOIs.Add(new Vector2Int(x, y));
                        SurroundPOIWithTerrain(x, y);
                    }
                }
            }
        }

        void SurroundPOIWithTerrain(int px, int py)
        {
            for (int x = px - 1; x <= px + 1; x++)
            {
                for (int y = py - 1; y <= py + 1; y++)
                {
                    if (!(x == px && y == py))
                    {
                        if (_mapTiles[x, y] != null && (_mapTiles[x, y] == grassTile || _mapTiles[x, y] == dirtTile))
                        {
                            _mapTiles[x, y] = wallTile;
                            _walkableArea[x, y] = false;
                        }
                    }
                }
            }
        }

        void PlaceFlora()
        {
            // Deferred until after map is rendered
        }

        void PlaceRocks()
        {
            if (rockTile == null) return;

            float rockXOffset = Random.Range(0f, 1000f);
            float rockYOffset = Random.Range(0f, 1000f);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (_mapTiles[x, y] == grassTile || _mapTiles[x, y] == dirtTile)
                    {
                        float noiseValue = Mathf.PerlinNoise((x + rockXOffset) * rockNoiseScale, (y + rockYOffset) * rockNoiseScale);
                        if (noiseValue >= rockMinNoise && noiseValue <= rockMaxNoise && Random.value < rockDensity)
                        {
                            _obstacleTiles[x, y] = rockTile;
                            _walkableArea[x, y] = false; // becomes an obstacle
                        }
                    }
                }
            }
        }

        void RenderFinalMap()
        {
            baseTilemap.ClearAllTiles();
            obstaclesTilemap.ClearAllTiles();
            floraTilemap.ClearAllTiles();

            // Render base and obstacles based on walkability
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (_mapTiles[x, y] == null) continue;

                    if (_walkableArea[x, y])
                    {
                        // Walkable tile goes to base layer
                        baseTilemap.SetTile(new Vector3Int(x, y, 0), _mapTiles[x, y]);
                    }
                    else
                    {
                        // Not walkable: goes to obstacles layer
                        TileBase obstacleToPlace = _obstacleTiles[x, y] != null ? _obstacleTiles[x, y] : _mapTiles[x, y];
                        obstaclesTilemap.SetTile(new Vector3Int(x, y, 0), obstacleToPlace);
                    }
                }
            }

            // Place flora on walkable tiles (grass/dirt) that are not obstructed
            if (floraTile != null)
            {
                float floraXOffset = Random.Range(0f, 1000f);
                float floraYOffset = Random.Range(0f, 1000f);

                for (int x = 1; x < width - 1; x++)
                {
                    for (int y = 1; y < height - 1; y++)
                    {
                        if (_walkableArea[x, y] && (_mapTiles[x, y] == grassTile || _mapTiles[x, y] == dirtTile))
                        {
                            float noiseValue = Mathf.PerlinNoise((x + floraXOffset) * floraNoiseScale, (y + floraYOffset) * floraNoiseScale);
                            if (noiseValue >= floraMinNoise && noiseValue <= floraMaxNoise && Random.value < floraDensity)
                            {
                                // Place flora only if there's no obstacle here
                                if (obstaclesTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
                                {
                                    floraTilemap.SetTile(new Vector3Int(x, y, 0), floraTile);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Place monster spawn points in a balanced way around the map.
        /// Similar logic to POI placement: random locations with minimum spacing.
        /// </summary>
        void PlaceMonsterSpawns()
        {
            if (monsterSpawnPrefab == null)
            {
                Debug.LogWarning("No monsterSpawnPrefab assigned. Skipping monster spawn placement.");
                return;
            }

            List<Vector2> placedSpawns = new List<Vector2>();
            int attempts = 0;
            int maxAttempts = numberOfMonsterSpawns * 100; // Arbitrary to avoid infinite loops

            while (placedSpawns.Count < numberOfMonsterSpawns && attempts < maxAttempts)
            {
                attempts++;
                int x = Random.Range(2, width - 2);
                int y = Random.Range(2, height - 2);

                // Check if tile is walkable and not obstructed
                if (_walkableArea[x, y] && obstaclesTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
                {
                    // Check distance from other spawn points
                    bool tooClose = false;
                    Vector2 candidatePos = new Vector2(x, y);
                    foreach (var spawnPos in placedSpawns)
                    {
                        if (Vector2.Distance(spawnPos, candidatePos) < minDistanceBetweenMonsterSpawns)
                        {
                            tooClose = true;
                            break;
                        }
                    }

                    if (!tooClose)
                    {
                        // Place monster spawn point
                        Vector3 worldPos = baseTilemap.CellToWorld(new Vector3Int(x, y, 0));
                        Instantiate(monsterSpawnPrefab, worldPos + new Vector3(0.5f, 0.5f, 0f), Quaternion.identity);
                        placedSpawns.Add(candidatePos);
                    }
                }
            }

            if (placedSpawns.Count < numberOfMonsterSpawns)
            {
                Debug.LogWarning("Not all monster spawns could be placed due to constraints.");
            }
            else
            {
                Debug.Log($"Placed {placedSpawns.Count} monster spawn points.");
            }
        }
    }
}
