using System.Collections.Generic;
using _Script.Map.Procedural;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Generators
{
    /// <summary>
    /// 负责地图逻辑的核心部分（网格、噪声分配、Biome 等）。
    /// 不包含任何与 Tilemap Renderer 相关的操作。
    /// </summary>
    public class MapTileLogic
    {
        // 尺寸
        private int _width;
        private int _height;
        
        // 随机种子
        private int _seed;
        private bool _useRandomSeed;

        // Biomes
        private Biome[] _biomes;
        
        // 噪声设置
        private float _biomeNoiseScale;
        private float _perlinScale;

        // 中间数据
        private TileData[,] _mapTiles; public TileData[,] MapTiles => _mapTiles;

        private bool[,] _walkableArea; 
        public bool[,] WalkableArea => _walkableArea;
        private TileData[,] _obstacleTiles; public TileData[,] ObstacleTiles => _obstacleTiles;
        private Biome[,] _tileBiomes; public Biome[,] TileBiomes => _tileBiomes;
        
        
        // 区域划分数据
        private List<List<Vector2Int>> _reachableAreas;
        private List<Vector2Int> _chosenRegion;

        // 辅助字段
        private Dictionary<Biome, List<Vector2Int>> _biomeTilesDict;
        public List<Vector2Int> ChosenRegion => _chosenRegion;
        public Dictionary<Biome, List<Vector2Int>> BiomeTilesDict => _biomeTilesDict;

        /// <summary>
        /// 初始化逻辑类，传入必要参数
        /// </summary>
        public MapTileLogic(int width, int height, int seed, bool useRandomSeed,
            Biome[] biomes, float biomeNoiseScale, float perlinScale)
        {
            _width = width;
            _height = height;
            _seed = seed;
            _useRandomSeed = useRandomSeed;
            _biomes = biomes;
            _biomeNoiseScale = biomeNoiseScale;
            _perlinScale = perlinScale;
        }

        /// <summary>
        /// 核心：执行地图生成流程（不包含实际的 Tilemap.SetTile 操作）
        /// </summary>
        public void GenerateMapLogic()
        {
            // 设置随机种子
            if (!_useRandomSeed)
                Random.InitState(_seed);
            else
                Random.InitState(System.DateTime.Now.GetHashCode());

            // 分配数组
            _mapTiles = new TileData[_width, _height];
            _walkableArea = new bool[_width, _height];
            _obstacleTiles = new TileData[_width, _height];
            _tileBiomes = new Biome[_width, _height];

            // 1) 地图边界初始化
            InitializeBoundary();

            // 2) 按噪声给 Biome
            AssignBiomesToTiles();

            // 3) 生成可行走区域 + 墙体
            GenerateWalkableArea();
            
            // 4) 应用 Biome 地形(草地/泥土等)
            ApplyBiomeTerrain();

            // 5) 应用 PerlinNoise 特征(森林/水域等)
            ApplyPerlinBasedFeatures();

            // 6) 放置POI(建筑/特殊点)
            //PlacePoIsFromBiomes();

            // 7) 放置石头(Obstacle)
            //PlaceRocksFromBiomes();

            // 同步最终结果到 WalkableArea
            //SyncWalkableAreaWithFinalMap();

            // 分析可达区域 & 选一个最大的区域
            IdentifyReachableAreas();
            _chosenRegion = SelectLargestRegion();

            // 建立 Biome->Tiles 字典，用于资源摆放或后续处理
            BuildBiomeTilesDictionary();
        }

        #region 地图各阶段逻辑

        private void InitializeBoundary()
        {
            // We only do a single pass to assign boundary tiles as non-walkable:
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    _mapTiles[x, y] = new TileData();
                    // Only the outer ring is non-walkable
                    _walkableArea[x, y] = !(x == 0 || y == 0 || x == _width - 1 || y == _height - 1);
                }
            }
        }

        private void AssignBiomesToTiles()
        {
            // Pre-calculate random offsets ONCE (instead of per-tile)
            float xOffset = Random.Range(0f, 9999f);
            float yOffset = Random.Range(0f, 9999f);

            // For each cell in [1.._width-1, 1.._height-1], get Perlin
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    float n = Mathf.PerlinNoise(
                        (x + xOffset) * _biomeNoiseScale,
                        (y + yOffset) * _biomeNoiseScale
                    );

                    // Store once to avoid repeated indexing
                    _tileBiomes[x, y] = PickBiomeByNoise(n);
                }
            }
        }

        private Biome PickBiomeByNoise(float n)
        {
            // 按阈值顺序选一个 Biome
            foreach (var b in _biomes)
            {
                if (n <= b.selectionThreshold)
                    return b;
            }
            return _biomes[_biomes.Length - 1];
        }

        private void GenerateWalkableArea()
        {
            // 1) 初始随机可行走
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    // We only call Random.value once per cell
                    float rv = Random.value;
                    // 70% chance walkable
                    _walkableArea[x, y] = (rv > 0.3f);
                }
            }

            // 2) 多次平滑
            for (int i = 0; i < 3; i++)
            {
                _walkableArea = SmoothWalkableArea(_walkableArea);
            }

            // 3) 将不能走的用 Biome 的 wallTile 来替换
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    if (_walkableArea[x, y])
                    {
                        _mapTiles[x, y].TileType = _tileBiomes[x, y].mainGroundTile;
                    }
                    else
                    {
                        // Store the biome reference in a local variable
                        Biome b = _tileBiomes[x, y];
                        _mapTiles[x, y] = new TileData
                        {
                            TileType = b.wallTile,
                        };
                    }
                }
            }
        }

        /**
         * Counts walkable neighbors for a given cell.
         * If more than 4 neighbors are walkable, keep it walkable
         */
        private bool[,] SmoothWalkableArea(bool[,] area)
        {
            bool[,] newArea = new bool[_width, _height];
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    int n = CountWalkableNeighbors(area, x, y);
                    // If more than 4 neighbors are walkable, keep it walkable
                    newArea[x, y] = (n > 4);
                }
            }
            return newArea;
        }

        private int CountWalkableNeighbors(bool[,] area, int cx, int cy)
        {
            int count = 0;
            // We can unroll or keep it as is
            for (int nx = cx - 1; nx <= cx + 1; nx++)
            {
                for (int ny = cy - 1; ny <= cy + 1; ny++)
                {
                    if (nx >= 0 && nx < _width && ny >= 0 && ny < _height)
                    {
                        if (area[nx, ny]) count++;
                    }
                }
            }
            return count;
        }

        private void ApplyBiomeTerrain()
        {
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    Biome b = _tileBiomes[x, y];
                    if (!b) continue;
                    
                    // Only apply main ground tile if currently walkable
                    if (_walkableArea[x, y])
                    {
                        _mapTiles[x, y] = new TileData
                        {
                            TileType = b.mainGroundTile,
                        };
                    }
                }
            }
        }

        private void ApplyPerlinBasedFeatures()
        {
            // Pre-calculate random offsets
            float xOffset = Random.Range(0f, 1000f);
            float yOffset = Random.Range(0f, 1000f);
            
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    // Store in local variable
                    Biome b = _tileBiomes[x, y];
                    if (!b) continue;

                    // If not walkable, no need to do Perlin check
                    if (!_walkableArea[x, y]) 
                        continue;

                    float val = Mathf.PerlinNoise(
                        (x + xOffset) * _perlinScale,
                        (y + yOffset) * _perlinScale
                    ) * 100f;
                    
                    /*--------Perlin --------- Noise--------*/
                    // Compare val just once
                    if(val < b.waterThreshold)
                    {
                        _mapTiles[x, y].TileType = b.waterTile;
                        _mapTiles[x, y].State = TileState.Surface;
                        _walkableArea[x, y] = false;
                    }
                    else if (val < b.mainGroundThreshold)
                    {
                        _mapTiles[x, y].TileType = b.mainGroundTile;
                        _mapTiles[x, y].State = TileState.Surface;
                        _walkableArea[x, y] = true;
                    }
                    else
                    {
                        _mapTiles[x, y].TileType = b.forestTile;
                    }
                }
            }
        }

        private void PlacePoIsFromBiomes()
        {
            foreach (var b in _biomes)
            {
                if (b.numberOfPOIs <= 0) continue;

                List<Vector2Int> placedPoIs = new List<Vector2Int>();
                int attempts = 0;
                int maxAttempts = b.numberOfPOIs * 50;

                while (placedPoIs.Count < b.numberOfPOIs && attempts < maxAttempts)
                {
                    attempts++;
                    int x = Random.Range(2, _width - 2);
                    int y = Random.Range(2, _height - 2);

                    // Store main map tile in local
                    var t = _mapTiles[x, y];

                    // Only place if it's mainGroundTile/forestTile/dirtTile
                    if (t.TileType == b.mainGroundTile || t.TileType == b.forestTile)
                    {
                        bool tooClose = false;
                        Vector2Int cand = new Vector2Int(x, y);

                        // Check distance to previously placed POIs
                        foreach (var p in placedPoIs)
                        {
                            if (Vector2Int.Distance(p, cand) < b.minDistanceBetweenPOIs)
                            {
                                tooClose = true;
                                break;
                            }
                        }
                        
                        if (!tooClose)
                        {
                            _mapTiles[x, y].TileType = b.poiTile;
                            placedPoIs.Add(cand);

                            // Surround with terrain
                            SurroundPoiWithTerrain(x, y, b);
                        }
                    }
                }
            }
        }

        private void SurroundPoiWithTerrain(int px, int py, Biome b)
        {
            for (int x = px - 1; x <= px + 1; x++)
            {
                for (int y = py - 1; y <= py + 1; y++)
                {
                    // Only change if not the center tile
                    if (x == px && y == py) continue;

                    // If surrounding tile is grass/dirt, turn it into a wall
                    var t = _mapTiles[x, y];
                    // if (t.TileType == b.grassTile || t.TileType == b.dirtTile)
                    // {
                    //     _mapTiles[x, y].TileType = b.wallTile;
                    //     _walkableArea[x, y] = false;
                    // }
                }
            }
        }

        private void PlaceRocksFromBiomes()
        {
            // Pre-calc random offsets
            float rockXOffset = Random.Range(0f, 1000f);
            float rockYOffset = Random.Range(0f, 1000f);

            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    Biome b = _tileBiomes[x, y];
                    if (b == null) continue;

                    var t = _mapTiles[x, y];
                    // Only place rock if tile is grass or dirt
                    // if (t.TileType == b.grassTile || t.TileType == b.dirtTile)
                    // {
                    //     float val = Mathf.PerlinNoise(
                    //         (x + rockXOffset) * b.rockNoiseScale,
                    //         (y + rockYOffset) * b.rockNoiseScale
                    //     );
                    //     // Check min/max noise and random
                    //     if (val >= b.rockMinNoise && val <= b.rockMaxNoise 
                    //         && Random.value < b.rockDensity)
                    //     {
                    //         _obstacleTiles[x, y].TileType = b.rockTile;
                    //         _walkableArea[x, y] = false;
                    //     }
                    // }
                }
            }
        }

        /// <summary>
        /// Sync walkable area with final map, ensuring obstacles, walls, water override walkable.
        /// </summary>
        private void SyncWalkableAreaWithFinalMap()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    // 1) If there is an obstacle tile, it overrides anything else:
                    var obstacle = _obstacleTiles[x, y];
                    _mapTiles[x, y] = obstacle;
                    _walkableArea[x, y] = false;
                }
            }
        }

        private void IdentifyReachableAreas()
        {
            _reachableAreas = new List<List<Vector2Int>>();
            bool[][] visited = new bool[_width][];
            for (int index = 0; index < _width; index++)
            {
                visited[index] = new bool[_height];
            }

            // BFS for each unvisited walkable tile
            for (int x = 1; x < _width - 1; x++)
            {
                for (int y = 1; y < _height - 1; y++)
                {
                    if (_walkableArea[x, y] && !visited[x][y])
                    {
                        var area = new List<Vector2Int>();
                        Queue<Vector2Int> queue = new Queue<Vector2Int>();

                        queue.Enqueue(new Vector2Int(x, y));
                        visited[x][y] = true;

                        while (queue.Count > 0)
                        {
                            Vector2Int current = queue.Dequeue();
                            area.Add(current);

                            // Check 4 neighbors
                            foreach (var dir in new Vector2Int[]
                                     {
                                         new Vector2Int(1, 0), 
                                         new Vector2Int(-1, 0),
                                         new Vector2Int(0, 1), 
                                         new Vector2Int(0, -1)
                                     })
                            {
                                int nx = current.x + dir.x;
                                int ny = current.y + dir.y;

                                // Quick bounds check
                                if (nx > 0 && nx < _width && ny > 0 && ny < _height)
                                {
                                    if (_walkableArea[nx, ny] && !visited[nx][ny])
                                    {
                                        visited[nx][ny] = true;
                                        queue.Enqueue(new Vector2Int(nx, ny));
                                    }
                                }
                            }
                        }

                        if (area.Count > 0)
                            _reachableAreas.Add(area);
                    }
                }
            }
        }

        private List<Vector2Int> SelectLargestRegion()
        {
            if (_reachableAreas.Count == 0)
            {
                return new List<Vector2Int>(); // fallback
            }

            // Track largest by area.Count
            List<Vector2Int> largest = _reachableAreas[0];
            foreach (var area in _reachableAreas)
            {
                if (area.Count > largest.Count)
                    largest = area;
            }
            return largest;
        }

        private void BuildBiomeTilesDictionary()
        {
            _biomeTilesDict = new Dictionary<Biome, List<Vector2Int>>();
            if (_chosenRegion == null) return;

            foreach (var pos in _chosenRegion)
            {
                Biome b = _tileBiomes[pos.x, pos.y];
                if (b == null) continue;

                if (!_biomeTilesDict.ContainsKey(b))
                {
                    _biomeTilesDict[b] = new List<Vector2Int>();
                }
                _biomeTilesDict[b].Add(pos);
            }
        }

        #endregion
        
    }
    
    public class TileData
    {
        public TileType TileType = TileType.None;
        public TileState State = TileState.Ground;
    }
    
    public enum TileState
    {
        Surface,
        Ground
    }
}