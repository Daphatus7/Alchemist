// Author : Peiyu Wang @ Daphatus
// 06 12 2024 12 55

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Threading.Tasks; // 用于 async/await
using Newtonsoft.Json; // 用于解析AI返回的JSON (需安装Newtonsoft.Json包)

namespace _Script.Map.Procedural
{
    public class ProceduralMapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")] public int mapWidth = 512;
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

        private float[,] heightMap; // 储存高度值（0-1）
        private float[,] moistureMap; // 储存湿度值（0-1）
        private BiomeData[,] biomeMap; // 对应每个格子所属的biome

        private List<Vector2Int> potentialFloraSpots = new List<Vector2Int>();
        private System.Random rand;

        async void Start()
        {
            rand = new System.Random();
            
            // 首先从AI获取调整后的Biome参数
            // 假设此方法会更新 biomes 数组内的参数
            await LoadBiomesFromAI();

            StartCoroutine(GenerateMapDataAsync());
        }

        // 使用异步方法从生成式AI获取参数
        async Task LoadBiomesFromAI()
        {
            // 这里是模拟Prompt，可以根据你的设计自由变化
            string prompt = "请为如下生物群系提供合理的参数范围（高度、湿度）和资源刷新概率，以JSON返回：" +
                            "biomes: [{name:'Forest'}, {name:'Grassland'}, {name:'Desert'}]";

            // 调用您封装的AI客户端
            string responseJson = await OpenAIClient.RequestAsync(prompt);

            // 假设AI返回类似的JSON格式:
            // {
            //    "biomes":[
            //       {
            //         "name":"Forest",
            //         "minHeight":0.3,"maxHeight":0.8,"minMoisture":0.5,"maxMoisture":1.0,
            //         "detailChance":0.15,"floraSpawnChance":0.1
            //       },
            //       {
            //         "name":"Grassland",
            //         "minHeight":0.1,"maxHeight":0.4,"minMoisture":0.3,"maxMoisture":0.7,
            //         "detailChance":0.05,"floraSpawnChance":0.05
            //       },
            //       {
            //         "name":"Desert",
            //         "minHeight":0.0,"maxHeight":0.2,"minMoisture":0.0,"maxMoisture":0.2,
            //         "detailChance":0.02,"floraSpawnChance":0.01
            //       }
            //    ]
            // }

            // 解析JSON
            var aiResult = JsonConvert.DeserializeObject<BiomesAIResult>(responseJson);
            if (aiResult != null && aiResult.biomes != null)
            {
                // 将AI生成的参数映射到当前biomes中
                foreach (var aiBiome in aiResult.biomes)
                {
                    for (int i = 0; i < biomes.Length; i++)
                    {
                        if (biomes[i].biomeName == aiBiome.name)
                        {
                            biomes[i].minHeight = aiBiome.minHeight;
                            biomes[i].maxHeight = aiBiome.maxHeight;
                            biomes[i].minMoisture = aiBiome.minMoisture;
                            biomes[i].maxMoisture = aiBiome.maxMoisture;
                            biomes[i].detailChance = aiBiome.detailChance;
                            biomes[i].floraSpawnChance = aiBiome.floraSpawnChance;
                            // 您还可以扩展更多AI控制的属性，如替换biomes[i].baseTile 等
                        }
                    }
                }
            }
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

            Debug.Log("Map Generation Complete with AI Adjusted Parameters!");
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
                    var prefab = biome.floraPrefabs[rand.Next(biome.floraPrefabs.Length)];
                    Vector3 worldPos = baseTilemap.CellToWorld(new Vector3Int(spot.x, spot.y, 0)) +
                                       new Vector3(0.5f, 0.5f, 0);
                    Instantiate(prefab, worldPos, Quaternion.identity);
                }
            }
        }
    }

    // 用于解析AI返回的JSON格式
    [System.Serializable]
    public class BiomeAIData
    {
        public string name;
        public float minHeight;
        public float maxHeight;
        public float minMoisture;
        public float maxMoisture;
        public float detailChance;
        public float floraSpawnChance;
    }

    [System.Serializable]
    public class BiomesAIResult
    {
        public BiomeAIData[] biomes;
    }

    // 假设的AI客户端类，仅作演示用
    public static class OpenAIClient
    {
        public static async Task<string> RequestAsync(string prompt)
        {
            // 在此实现实际的HTTP请求，如使用UnityWebRequest异步调用OpenAI API
            // 这里简单返回一个模拟的JSON
            await Task.Delay(500); // 模拟网络延迟
            return @"{
                ""biomes"": [
                    {""name"":""Forest"", ""minHeight"":0.3,""maxHeight"":0.8,""minMoisture"":0.5,""maxMoisture"":1.0,""detailChance"":0.15,""floraSpawnChance"":0.1},
                    {""name"":""Grassland"", ""minHeight"":0.1,""maxHeight"":0.4,""minMoisture"":0.3,""maxMoisture"":0.7,""detailChance"":0.05,""floraSpawnChance"":0.05},
                    {""name"":""Desert"", ""minHeight"":0.0,""maxHeight"":0.2,""minMoisture"":0.0,""maxMoisture"":0.2,""detailChance"":0.02,""floraSpawnChance"":0.01}
                ]
            }";
        }
    }
}
