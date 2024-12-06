// Author : Peiyu Wang @ Daphatus
// 06 12 2024 12 55

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Procedural
{
    public class ComplexMapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")]
        public int mapWidth = 512;
        public int mapHeight = 512;
        public int chunkSize = 64; 
        public int chunksToGenerateX = 8;
        public int chunksToGenerateY = 8;

        [Header("Noise Settings")]
        public float baseNoiseScale = 0.02f;        // Increased for more noticeable variations
        public float moistureNoiseScale = 0.02f;    // Increased for more noticeable variations

        // Adding a seed for deterministic variation
        [Header("Seed")]
        public int seed = 12345;  // You can randomize this if desired

        [Header("Biomes")]
        public BiomeData[] biomes;

        [Header("Tilemaps")]
        public Tilemap baseTilemap;
        public Tilemap detailTilemap;

        [Header("Others")]
        public float plantRefreshInterval = 10f; 

        private float[,] heightMap;    
        private float[,] moistureMap;  
        private BiomeData[,] biomeMap; 

        private List<Vector2Int> potentialFloraSpots = new List<Vector2Int>();
        private System.Random rand;

        void Start()
        {
            rand = new System.Random(seed);
            StartCoroutine(GenerateMapDataAsync());
        }

        IEnumerator GenerateMapDataAsync()
        {
            // Allocate arrays
            heightMap = new float[mapWidth, mapHeight];
            moistureMap = new float[mapWidth, mapHeight];
            biomeMap = new BiomeData[mapWidth, mapHeight];

            // Generate base noise data
            GenerateHeightMap();
            GenerateMoistureMap();

            // Classify biomes
            ClassifyBiomes();

            // Draw chunks asynchronously
            yield return StartCoroutine(DrawChunksAsync());

            // If you have a composite collider, update it
            var composite = baseTilemap.GetComponent<UnityEngine.CompositeCollider2D>();
            if (composite != null)
            {
                composite.GenerateGeometry();
            }

            FindFloraSpots();
            StartCoroutine(RefreshFloraPeriodically());

            Debug.Log("Map Generation Complete!");
        }

        void GenerateHeightMap()
        {
            // Create offsets from seed for height
            float hOffsetX = seed + 1000f;
            float hOffsetY = seed + 2000f;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    float nx = (x + hOffsetX) * baseNoiseScale;
                    float ny = (y + hOffsetY) * baseNoiseScale;
                    float sample = Mathf.PerlinNoise(nx, ny);
                    heightMap[x, y] = sample;
                }
            }
        }

        void GenerateMoistureMap()
        {
            // Create offsets from seed for moisture
            float mOffsetX = seed + 3000f;
            float mOffsetY = seed + 4000f;
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    float nx = (x + mOffsetX) * moistureNoiseScale;
                    float ny = (y + mOffsetY) * moistureNoiseScale;
                    float sample = Mathf.PerlinNoise(nx, ny);
                    moistureMap[x, y] = sample;
                }
            }
        }

        void ClassifyBiomes()
        {
            // Ensure your biome thresholds are distinct and cover different ranges
            // Example: 
            // Biome 1: height 0.0-0.4, moisture 0.0-0.5
            // Biome 2: height 0.4-0.7, moisture 0.0-0.5
            // Biome 3: height 0.7-1.0, moisture 0.0-0.5
            // Biome 4: height 0.0-0.4, moisture 0.5-1.0
            // ... etc.
            // Adjust biome ScriptableObjects accordingly.

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    float h = heightMap[x, y];
                    float m = moistureMap[x, y];
                    biomeMap[x, y] = SelectBiome(h, m);
                }
            }
        }

        BiomeData SelectBiome(float height, float moisture)
        {
            foreach (var b in biomes)
            {
                if (height >= b.minHeight && height <= b.maxHeight &&
                    moisture >= b.minMoisture && moisture <= b.maxMoisture)
                {
                    return b;
                }
            }

            // Fallback if no biome is matched (should be avoided by having properly set thresholds)
            return biomes.Length > 0 ? biomes[0] : null;
        }

        IEnumerator DrawChunksAsync()
        {
            int totalChunksX = Mathf.CeilToInt((float)mapWidth / chunkSize);
            int totalChunksY = Mathf.CeilToInt((float)mapHeight / chunkSize);

            for (int cx = 0; cx < totalChunksX && cx < chunksToGenerateX; cx++)
            {
                for (int cy = 0; cy < totalChunksY && cy < chunksToGenerateY; cy++)
                {
                    DrawChunk(cx, cy);
                    yield return null;
                }
            }
        }

        void DrawChunk(int chunkX, int chunkY)
        {
            int startX = chunkX * chunkSize;
            int startY = chunkY * chunkSize;
            int endX = Mathf.Min(startX + chunkSize, mapWidth);
            int endY = Mathf.Min(startY + chunkSize, mapHeight);

            List<Vector3Int> basePositions = new List<Vector3Int>();
            List<TileBase> baseTiles = new List<TileBase>();

            List<Vector3Int> detailPositions = new List<Vector3Int>();
            List<TileBase> detailTilesArray = new List<TileBase>();

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    var biome = biomeMap[x, y];
                    if (biome == null) continue;

                    Vector3Int pos = new Vector3Int(x, y, 0);
                    basePositions.Add(pos);
                    baseTiles.Add(biome.baseTile);

                    if (biome.detailTiles != null && biome.detailTiles.Length > 0 && 
                        rand.NextDouble() < biome.detailChance)
                    {
                        detailPositions.Add(pos);
                        detailTilesArray.Add(biome.detailTiles[rand.Next(biome.detailTiles.Length)]);
                    }
                }
            }

            baseTilemap.SetTiles(basePositions.ToArray(), baseTiles.ToArray());
            detailTilemap.SetTiles(detailPositions.ToArray(), detailTilesArray.ToArray());
        }

        void FindFloraSpots()
        {
            potentialFloraSpots.Clear();
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var biome = biomeMap[x, y];
                    if (biome == null) continue;
                    if (biome.floraPrefabs != null && biome.floraPrefabs.Length > 0)
                    {
                        potentialFloraSpots.Add(new Vector2Int(x, y));
                    }
                }
            }
        }

        IEnumerator RefreshFloraPeriodically()
        {
            while (true)
            {
                yield return new WaitForSeconds(plantRefreshInterval);
                RefreshFlora();
            }
        }

        void RefreshFlora()
        {
            for (int i = 0; i < 50; i++)
            {
                if (potentialFloraSpots.Count == 0) break;
                int idx = rand.Next(potentialFloraSpots.Count);
                Vector2Int spot = potentialFloraSpots[idx];

                var biome = biomeMap[spot.x, spot.y];
                if (biome == null || biome.floraPrefabs == null || biome.floraPrefabs.Length == 0) continue;

                if (rand.NextDouble() < biome.floraSpawnChance)
                {
                    Vector3 worldPos = baseTilemap.CellToWorld(new Vector3Int(spot.x, spot.y, 0)) + new Vector3(0.5f, 0.5f, 0);
                    Instantiate(biome.floraPrefabs[rand.Next(biome.floraPrefabs.Length)], worldPos, Quaternion.identity);
                }
            }
        }
    }
}
