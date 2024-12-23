using System;
using System.Collections.Generic;
using _Script.Map.Procedural;
using _Script.Map.Tile.Tile_Base;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

namespace _Script.Map.Generators
{
    public class ProceduralMapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")]
        public int width = 50;
        public int height = 50;
        
        [Serializable]
        public class TileTypeTileBasePair
        {
            public TileType tileType;
            public SpriteAtlas tileSet;
            public string name;
        }
        
        public List<TileTypeTileBasePair> tileSet;
        
        [Header("Base Tilemap References")]
        public Tilemap baseTilemap;
        public Tilemap obstaclesTilemap;
        public Tilemap floraTilemap;

        [Header("Debug")]
        public Tilemap debugTilemap;
        public TileBase debugTile;

        [Header("Biomes")]
        public Biome[] biomes;

        [Header("Noise Settings")]
        [Range(0f,1f)] public float biomeNoiseScale = 0.1f;
        [Range(0f,1f)] public float perlinScale = 0.1f;
        public int seed = 0;
        public bool useRandomSeed = false;

        // 逻辑层 & 渲染层 & 资源生成器
        private MapTileLogic _mapLogic;
        private MapTileRenderer _mapRenderer;
        private ResourceGenerator _resourceGenerator;

        // 记录生成后的起点 / 终点
        private Vector2Int _spawnPoint; 
        public Vector2Int SpawnPoint => _spawnPoint;
        private Vector2Int _endPoint; 
        public Vector2Int EndPoint => _endPoint;

        // 额外参数
        [SerializeField] private int _minDistance = 10;

        // ====== 主流程入口 ======
        
        
        [Button("Generate Map")]
        public void DebugGenerateMap()
        {
            GenerateMap(width, height, out _, out _);
        }
        
        public bool GenerateMap(int intWidth, int intHeight, out Vector2Int sPoint, out Vector2Int ePoint)
        {
            sPoint = Vector2Int.zero;
            ePoint = Vector2Int.zero;

            // 1) 检查引用
            if (baseTilemap == null || floraTilemap == null || obstaclesTilemap == null)
            {
                Debug.LogError("Assign baseTilemap, floraTilemap, and obstaclesTilemap.");
                return false;
            }
            if (biomes == null || biomes.Length == 0)
            {
                Debug.LogError("No biomes defined. Please assign biomes.");
                return false;
            }

            // 2) 初始化尺寸
            width = intWidth;
            height = intHeight;

            // 3) 构建逻辑层
            _mapLogic = new MapTileLogic(width, height, seed, useRandomSeed, 
                                         biomes, biomeNoiseScale, perlinScale);

            var tileSets = new Dictionary<TileType, TileBase[]>();
            // 4) 调用逻辑层生成
            _mapLogic.GenerateMapLogic();

            var tileDictionary = new Dictionary<TileType, TileBase[]>();
            
            foreach (var pair in tileSet)
            {
                TileBase[] tileArray = new TileBase[pair.tileSet.spriteCount];
                for (int i = 0; i < pair.tileSet.spriteCount; i++)
                {
                    string spriteName = $"{pair.name}_{i}";
                    Sprite s = pair.tileSet.GetSprite(spriteName);

                    // Create a new tile
                    UnityEngine.Tilemaps.Tile tile = ScriptableObject.CreateInstance<UnityEngine.Tilemaps.Tile>();
                    tile.sprite = s;
                    tileArray[i] = tile;
                }
                tileDictionary[pair.tileType] = tileArray;
            }
            
            // 5) 渲染层
            _mapRenderer = new MapTileRenderer(baseTilemap, obstaclesTilemap, floraTilemap, tileDictionary);
            
            
            _mapRenderer.RenderFinalMap(_mapLogic, tileDictionary);
            //_mapRenderer.PlaceFlora(_mapLogic);

            // 6) 生成怪物（如果需要），这里保留原始逻辑或者你拆成一个 MonsterGenerator.cs
            PlaceMonstersFromBiomes();  // 示例保留

            // 7) 资源生成器
            // _resourceGenerator = new ResourceGenerator();
            // _resourceGenerator.PlaceResourcesFromBiomes(
            //     biomes, 
            //     _mapLogic.BiomeTilesDict, 
            //     _mapLogic.ChosenRegion, 
            //     baseTilemap);

            // 8) 最后生成 Spawn / End
            GenerateSpawnAndEndPoint(_minDistance, out _spawnPoint, out _endPoint);
            sPoint = _spawnPoint;
            ePoint = _endPoint;

            return true;
        }

        // ====== 生成 spawn / end 点 ======
        public void GenerateSpawnAndEndPoint(float minimumDistance, out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            var chosenRegion = _mapLogic.ChosenRegion;
            if (chosenRegion == null || chosenRegion.Count < 2)
            {
                Debug.LogWarning("No largest reachable area found. Using fallback.");
                spawnPoint = new Vector2Int(1,1);
                endPoint = new Vector2Int(width-2,height-2);
                return;
            }

            int maxAttempts = 500;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2Int candidateSpawn = chosenRegion[Random.Range(0, chosenRegion.Count)];
                List<Vector2Int> farEnoughTiles = new List<Vector2Int>();
                foreach (var tile in chosenRegion)
                {
                    if (Vector2Int.Distance(tile, candidateSpawn) >= minimumDistance)
                    {
                        farEnoughTiles.Add(tile);
                    }
                }

                if (farEnoughTiles.Count > 0)
                {
                    Vector2Int candidateEnd = farEnoughTiles[Random.Range(0, farEnoughTiles.Count)];
                    spawnPoint = candidateSpawn;
                    endPoint = candidateEnd;
                    return;
                }
            }

            Debug.LogWarning("Could not find suitable spawn/end. Using fallback.");
            spawnPoint = chosenRegion[0];
            endPoint = chosenRegion[chosenRegion.Count - 1];
        }

        // ====== 放置怪物的示例逻辑（可继续拆分）======
        private void PlaceMonstersFromBiomes()
        {
            var chosenRegion = _mapLogic.ChosenRegion;
            if(chosenRegion == null || chosenRegion.Count == 0) return;

            foreach(var b in biomes)
            {
                if(b.monsterPrefab == null || b.numberOfMonsters <= 0) continue;

                var candidateList = new List<Vector2Int>(chosenRegion);
                int attempts=0;
                List<Vector2> placedM = new List<Vector2>();

                while(placedM.Count < b.numberOfMonsters && candidateList.Count>0 && 
                      attempts< b.numberOfMonsters*100)
                {
                    attempts++;
                    int randIndex=Random.Range(0,candidateList.Count);
                    Vector2Int spot=candidateList[randIndex];
                    bool tooClose=false;
                    foreach(var mPos in placedM)
                    {
                        if(Vector2.Distance(mPos,spot)<b.minMonsterDistance)
                        {
                            tooClose=true;
                            break;
                        }
                    }

                    if(!tooClose)
                    {
                        Vector3 wPos=baseTilemap.CellToWorld(new Vector3Int(spot.x,spot.y,0));
                        Instantiate(b.monsterPrefab,wPos+new Vector3(0.5f,0.5f,0f),Quaternion.identity);
                        placedM.Add(spot);
                        candidateList.RemoveAt(randIndex);
                    }
                    else
                    {
                        candidateList.RemoveAt(randIndex);
                    }
                }
            }
        }

        // ====== Debug 清理 ======
        public void DebugClearMap()
        {
            if(baseTilemap)      baseTilemap.ClearAllTiles();
            if(obstaclesTilemap) obstaclesTilemap.ClearAllTiles();
            if(floraTilemap)     floraTilemap.ClearAllTiles();
            if(debugTilemap)     debugTilemap.ClearAllTiles();

            _mapLogic = null;
            _mapRenderer = null;
            _resourceGenerator = null;

            _spawnPoint = Vector2Int.zero;
            _endPoint   = Vector2Int.zero;

            Debug.Log("Map and internal data have been cleared.");
        }
    }
}
