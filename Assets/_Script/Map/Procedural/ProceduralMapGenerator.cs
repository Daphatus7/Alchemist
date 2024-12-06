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
        public int chunkSize = 64; // 分块大小
        public int chunksToGenerateX = 8;
        public int chunksToGenerateY = 8;

        [Header("Noise Settings")]
        public float baseNoiseScale = 0.01f;
        public float moistureNoiseScale = 0.01f;
        // 可加更多通道，如温度、植被密度等

        [Header("Biomes")]
        public BiomeData[] biomes;

        [Header("Tilemaps")]
        public Tilemap baseTilemap;
        public Tilemap detailTilemap;

        [Header("Others")]
        public float plantRefreshInterval = 10f; // 植物定期刷新间隔（秒）

        private float[,] heightMap;    // 储存高度值（0-1）
        private float[,] moistureMap;  // 储存湿度值（0-1）
        private BiomeData[,] biomeMap; // 对应每个格子所属的biome

        private List<Vector2Int> potentialFloraSpots = new List<Vector2Int>();
        private System.Random rand;

        void Start()
        {
            rand = new System.Random();
            StartCoroutine(GenerateMapDataAsync());
        }

        IEnumerator GenerateMapDataAsync()
        {
            // 分配数组内存
            heightMap = new float[mapWidth, mapHeight];
            moistureMap = new float[mapWidth, mapHeight];
            biomeMap = new BiomeData[mapWidth, mapHeight];

            // 生成基础噪声数据（高度、湿度）
            GenerateHeightMap();
            GenerateMoistureMap();

            // 根据噪声数据分类biome
            ClassifyBiomes();

            // 按分块生成Tile，减少一次性绘制造成的卡顿
            yield return StartCoroutine(DrawChunksAsync());

            // 合并碰撞(如需)
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
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    float sample = Mathf.PerlinNoise(x * baseNoiseScale, y * baseNoiseScale);
                    heightMap[x, y] = sample;
                }
            }
        }

        void GenerateMoistureMap()
        {
            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    float sample = Mathf.PerlinNoise(x * moistureNoiseScale + 1000f, y * moistureNoiseScale + 1000f);
                    moistureMap[x, y] = sample;
                }
            }
        }

        void ClassifyBiomes()
        {
            // 根据高度和湿度将每个格子匹配到最合适的Biomes
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
            // 简单线性搜索，实际可优化或根据权重选择
            foreach (var b in biomes)
            {
                if (height >= b.minHeight && height <= b.maxHeight &&
                    moisture >= b.minMoisture && moisture <= b.maxMoisture)
                {
                    return b;
                }
            }

            // 如果没有匹配，返回一个默认biome（可选）
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
                    // 基础Tile
                    basePositions.Add(pos);
                    baseTiles.Add(biome.baseTile);

                    // 随机细节Tile
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
                    // 将属于该biome并有一定概率生长植物的地点加入列表
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
            // 从potentialFloraSpots中随机挑选一些点，尝试种植植物
            for (int i = 0; i < 50; i++)
            {
                if (potentialFloraSpots.Count == 0) break;
                int idx = rand.Next(potentialFloraSpots.Count);
                Vector2Int spot = potentialFloraSpots[idx];

                var biome = biomeMap[spot.x, spot.y];
                if (biome == null || biome.floraPrefabs == null || biome.floraPrefabs.Length == 0) continue;

                if (rand.NextDouble() < biome.floraSpawnChance)
                {
                    // 实例化植物
                    var prefab = biome.floraPrefabs[rand.Next(biome.floraPrefabs.Length)];
                    Vector3 worldPos = baseTilemap.CellToWorld(new Vector3Int(spot.x, spot.y, 0)) + new Vector3(0.5f, 0.5f, 0);
                    Instantiate(prefab, worldPos, Quaternion.identity);
                }
            }
        }
    }
}
