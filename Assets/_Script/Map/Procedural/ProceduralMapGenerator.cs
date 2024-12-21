using System.Collections.Generic;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;
using UnityEngine.Tilemaps;
using _Script.Map.Procedural.BiomeData;
using Sirenix.OdinInspector; // 记得引入

namespace _Script.Map.Procedural
{
    public class ProceduralMapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")]
        public int width = 50;
        public int height = 50;

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

        private TileBase[,] _mapTiles;
        private bool[,] _walkableArea;
        private TileBase[,] _obstacleTiles;
        private Biome[,] _tileBiomes;

        private List<List<Vector2Int>> reachableAreas;
        private List<Vector2Int> chosenRegion;

        private Vector2Int _spawnPoint; 
        public Vector2Int SpawnPoint => _spawnPoint;
        private Vector2Int _endPoint; 
        public Vector2Int EndPoint => _endPoint;

        [SerializeField] private int _minDistance = 10;

        // 新增：Biome -> 该Biome所有可通行格子
        private Dictionary<Biome, List<Vector2Int>> _biomeTilesDict;

        [Button]
        public bool GenerateMap(int intWidth, int iniHeight, out Vector2Int sPoint, out Vector2Int ePoint)
        {
            
            if (baseTilemap == null || floraTilemap == null || obstaclesTilemap == null)
            {
                Debug.LogError("Assign baseTilemap, floraTilemap, and obstaclesTilemap.");
                sPoint = Vector2Int.zero;
                ePoint = Vector2Int.zero;
                return false;
            }

            if (biomes == null || biomes.Length == 0)
            {
                Debug.LogError("No biomes defined. Please assign biomes.");
                sPoint = Vector2Int.zero;
                ePoint = Vector2Int.zero;
                return false;
            }

            if (!useRandomSeed)
                Random.InitState(seed);
            else
                Random.InitState(System.DateTime.Now.GetHashCode());
            
            width = intWidth;
            height = iniHeight;

            _mapTiles = new TileBase[width, height];
            _walkableArea = new bool[width, height];
            _obstacleTiles = new TileBase[width, height];
            _tileBiomes = new Biome[width, height];

            InitializeBoundary();
            AssignBiomesToTiles();
            GenerateWalkableArea();
            ApplyBiomeTerrain();
            ApplyPerlinBasedFeatures();
            PlacePOIsFromBiomes();
            PlaceRocksFromBiomes();
            RenderFinalMap();
            PlaceFloraFromBiomes();

            SyncWalkableAreaWithFinalMap();
            IdentifyReachableAreas();
            chosenRegion = SelectLargestRegion();

            PlaceMonstersFromBiomes();

            // 1) 先建立Biome -> 可走地块的缓存
            BuildBiomeTilesDictionary();

            // 2) 在构建完后再调用资源放置
            PlaceResourcesFromBiomes();

            GenerateSpawnAndEndPoint(_minDistance, out Vector2Int spawnPoint, out Vector2Int endPoint);
            _spawnPoint = spawnPoint;
            sPoint = spawnPoint;
            _endPoint = endPoint;
            ePoint = endPoint;

            return true;
        }
        
        [Button("Debug: Clear Map")]
        public void DebugClearMap()
        {
            // 1) 清空 Tilemap
            if(baseTilemap)      baseTilemap.ClearAllTiles();
            if(obstaclesTilemap) obstaclesTilemap.ClearAllTiles();
            if(floraTilemap)     floraTilemap.ClearAllTiles();
            if(debugTilemap)     debugTilemap.ClearAllTiles();

            // 2) 重置或清空相关内部数据
            _mapTiles      = null;
            _walkableArea  = null;
            _obstacleTiles = null;
            _tileBiomes    = null;

            reachableAreas?.Clear();
            chosenRegion?.Clear();
            _biomeTilesDict?.Clear();

            // 3) 重置 Spawn / End
            _spawnPoint = Vector2Int.zero;
            _endPoint   = Vector2Int.zero;

            // 如需销毁场景中生成的怪物或资源对象，可在此处补充
            // 例如：
            // DestroyAllSpawnedObjectsWithTag("Monster");
            // DestroyAllSpawnedObjectsWithTag("Resource");

            Debug.Log("Map and internal data have been cleared.");
        }

        void InitializeBoundary()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _mapTiles[x, y] = null;
                    _walkableArea[x, y] = !(x == 0 || y == 0 || x == width - 1 || y == height - 1);
                }
            }
        }

        void AssignBiomesToTiles()
        {
            float xOffset = Random.Range(0f,9999f);
            float yOffset = Random.Range(0f,9999f);

            for (int x = 1; x < width-1; x++)
            {
                for (int y = 1; y < height-1; y++)
                {
                    float n = Mathf.PerlinNoise((x+xOffset)*biomeNoiseScale, (y+yOffset)*biomeNoiseScale);
                    _tileBiomes[x,y] = PickBiomeByNoise(n);
                }
            }
        }

        private Biome PickBiomeByNoise(float n)
        {
            foreach (var b in biomes)
            {
                if (n <= b.selectionThreshold)
                    return b;
            }
            return biomes[biomes.Length-1];
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
                _walkableArea = SmoothWalkableArea(_walkableArea);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (_walkableArea[x, y])
                    {
                        _mapTiles[x,y] = null; 
                    }
                    else
                    {
                        Biome b = _tileBiomes[x,y];
                        _mapTiles[x,y] = b != null && b.wallTile != null ? b.wallTile : null;
                    }
                }
            }
        }

        bool[,] SmoothWalkableArea(bool[,] area)
        {
            bool[,] newArea = new bool[width,height];
            for (int x = 1; x < width-1; x++)
            {
                for (int y = 1; y < height-1; y++)
                {
                    int n = CountWalkableNeighbors(area,x,y);
                    newArea[x,y] = n > 4;
                }
            }
            return newArea;
        }

        int CountWalkableNeighbors(bool[,] area,int cx,int cy)
        {
            int count = 0;
            for (int nx=cx-1; nx<=cx+1; nx++)
            {
                for (int ny=cy-1; ny<=cy+1; ny++)
                {
                    if (nx>=0 && nx<width && ny>=0 && ny<height)
                    {
                        if (area[nx,ny]) count++;
                    }
                }
            }
            return count;
        }

        void ApplyBiomeTerrain()
        {
            for (int x=1; x<width-1; x++)
            {
                for (int y=1; y<height-1; y++)
                {
                    Biome b = _tileBiomes[x,y];
                    if (b == null) continue;
                    if (_walkableArea[x,y])
                        _mapTiles[x,y] = b.mainGroundTile;
                }
            }
        }

        void ApplyPerlinBasedFeatures()
        {
            float xOffset = Random.Range(0f,1000f);
            float yOffset = Random.Range(0f,1000f);

            for (int x=1; x<width-1; x++)
            {
                for (int y=1; y<height-1; y++)
                {
                    Biome b = _tileBiomes[x,y];
                    if (b == null || _mapTiles[x,y]==null || !_walkableArea[x,y]) continue;

                    float val = Mathf.PerlinNoise((x+xOffset)*perlinScale, (y+yOffset)*perlinScale)*100f;
                    if (val > b.forestThreshold && b.forestTile!=null)
                    {
                        _mapTiles[x,y] = b.forestTile;
                        _walkableArea[x,y] = false;
                    }
                    else if (val < b.waterThreshold && b.waterTile!=null)
                    {
                        _mapTiles[x,y] = b.waterTile;
                        _walkableArea[x,y] = false;
                    }
                }
            }
        }

        void PlacePOIsFromBiomes()
        {
            foreach (var b in biomes)
            {
                if (b.poiTile == null || b.numberOfPOIs <=0) continue;

                List<Vector2Int> placedPOIs = new List<Vector2Int>();
                int attempts = 0;
                int maxAttempts = b.numberOfPOIs*50;

                while (placedPOIs.Count < b.numberOfPOIs && attempts<maxAttempts)
                {
                    attempts++;
                    int x=Random.Range(2,width-2);
                    int y=Random.Range(2,height-2);
                    if (_mapTiles[x,y]==b.mainGroundTile || _mapTiles[x,y]==b.forestTile || _mapTiles[x,y]==b.dirtTile)
                    {
                        bool tooClose=false;
                        Vector2Int cand=new Vector2Int(x,y);
                        foreach(var p in placedPOIs)
                        {
                            if (Vector2Int.Distance(p,cand)<b.minDistanceBetweenPOIs)
                            {
                                tooClose=true;
                                break;
                            }
                        }
                        if(!tooClose)
                        {
                            _mapTiles[x,y]=b.poiTile;
                            placedPOIs.Add(cand);
                            SurroundPOIWithTerrain(x,y,b);
                        }
                    }
                }
            }
        }

        void SurroundPOIWithTerrain(int px,int py,Biome b)
        {
            for (int x=px-1;x<=px+1;x++)
            {
                for (int y=py-1;y<=py+1;y++)
                {
                    if(!(x==px && y==py))
                    {
                        if (_mapTiles[x,y]==b.grassTile || _mapTiles[x,y]==b.dirtTile)
                        {
                            if (b.wallTile!=null)
                            {
                                _mapTiles[x,y]=b.wallTile;
                                _walkableArea[x,y]=false;
                            }
                        }
                    }
                }
            }
        }

        void PlaceRocksFromBiomes()
        {
            float rockXOffset = Random.Range(0f,1000f);
            float rockYOffset = Random.Range(0f,1000f);

            for (int x=1;x<width-1;x++)
            {
                for (int y=1;y<height-1;y++)
                {
                    Biome b=_tileBiomes[x,y];
                    if (b==null || b.rockTile==null) continue;

                    if (_mapTiles[x,y]==b.grassTile || _mapTiles[x,y]==b.dirtTile)
                    {
                        float val=Mathf.PerlinNoise((x+rockXOffset)*b.rockNoiseScale,(y+rockYOffset)*b.rockNoiseScale);
                        if(val>=b.rockMinNoise && val<=b.rockMaxNoise && Random.value<b.rockDensity)
                        {
                            _obstacleTiles[x,y]=b.rockTile;
                            _walkableArea[x,y]=false;
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

            for (int x=0;x<width;x++)
            {
                for (int y=0;y<height;y++)
                {
                    TileBase ground=_mapTiles[x,y];
                    if (ground==null) continue;

                    if (_walkableArea[x,y])
                        baseTilemap.SetTile(new Vector3Int(x,y,0),ground);
                    else
                    {
                        TileBase obstacle = _obstacleTiles[x,y]!=null?_obstacleTiles[x,y]:ground;
                        obstaclesTilemap.SetTile(new Vector3Int(x,y,0),obstacle);
                    }
                }
            }
        }

        void PlaceFloraFromBiomes()
        {
            for(int x=1;x<width-1;x++)
            {
                for(int y=1;y<height-1;y++)
                {
                    Biome b=_tileBiomes[x,y];
                    if(b==null||b.floraTile==null) continue;

                    if(_walkableArea[x,y] && obstaclesTilemap.GetTile(new Vector3Int(x,y,0))==null)
                    {
                        float fXOff=Random.Range(0f,1000f);
                        float fYOff=Random.Range(0f,1000f);
                        float noise=Mathf.PerlinNoise((x+fXOff)*b.floraNoiseScale,(y+fYOff)*b.floraNoiseScale);

                        if(noise>=b.floraMinNoise && noise<=b.floraMaxNoise && Random.value<b.floraDensity)
                        {
                            floraTilemap.SetTile(new Vector3Int(x,y,0),b.floraTile);
                        }
                    }
                }
            }
        }

        void SyncWalkableAreaWithFinalMap()
        {
            for (int x=0;x<width;x++)
            {
                for (int y=0;y<height;y++)
                {
                    if (_mapTiles[x,y]==null)
                    {
                        _walkableArea[x,y]=false;
                        continue;
                    }

                    Biome b=_tileBiomes[x,y];
                    if (b!=null && (_mapTiles[x,y]==b.waterTile||_mapTiles[x,y]==b.wallTile))
                    {
                        _walkableArea[x,y]=false;
                        continue;
                    }

                    if (obstaclesTilemap.GetTile(new Vector3Int(x,y,0))!=null)
                    {
                        _walkableArea[x,y]=false;
                        continue;
                    }

                    _walkableArea[x,y]=true;
                }
            }
        }

        void IdentifyReachableAreas()
        {
            reachableAreas = new List<List<Vector2Int>>();
            bool[,] visited = new bool[width,height];

            for (int x=1; x<width-1; x++)
            {
                for (int y=1; y<height-1; y++)
                {
                    if(_walkableArea[x,y] && !visited[x,y])
                    {
                        var area = new List<Vector2Int>();
                        Queue<Vector2Int> queue = new Queue<Vector2Int>();
                        queue.Enqueue(new Vector2Int(x,y));
                        visited[x,y]=true;

                        while(queue.Count>0)
                        {
                            Vector2Int current = queue.Dequeue();
                            area.Add(current);

                            foreach(var dir in new Vector2Int[]{
                                        new Vector2Int(1,0),new Vector2Int(-1,0),
                                        new Vector2Int(0,1),new Vector2Int(0,-1)})
                            {
                                int nx=current.x+dir.x;
                                int ny=current.y+dir.y;
                                if(nx>0 && nx<width && ny>0 && ny<height)
                                {
                                    if(_walkableArea[nx,ny] && !visited[nx,ny])
                                    {
                                        visited[nx,ny]=true;
                                        queue.Enqueue(new Vector2Int(nx,ny));
                                    }
                                }
                            }
                        }

                        if(area.Count>0)
                            reachableAreas.Add(area);
                    }
                }
            }
        }
        
        List<Vector2Int> SelectLargestRegion()
        {
            if(reachableAreas.Count==0)
            {
                // fallback
                return new List<Vector2Int>();
            }

            List<Vector2Int> largest = reachableAreas[0];
            foreach(var area in reachableAreas)
            {
                if(area.Count>largest.Count)
                    largest=area;
            }
            return largest;
        }

        void PlaceMonstersFromBiomes()
        {
            if(chosenRegion==null || chosenRegion.Count==0) return;

            foreach(var b in biomes)
            {
                if(b.monsterPrefab==null||b.numberOfMonsters<=0) continue;

                var candidateList = new List<Vector2Int>(chosenRegion);
                int attempts=0;
                List<Vector2> placedM = new List<Vector2>();

                while(placedM.Count < b.numberOfMonsters && candidateList.Count>0 && attempts< b.numberOfMonsters*100)
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

        public void GenerateSpawnAndEndPoint(float minimumDistance, out Vector2Int spawnPoint, out Vector2Int endPoint)
        {
            if (chosenRegion == null || chosenRegion.Count < 2)
            {
                Debug.LogWarning("No largest reachable area found or not enough tiles for spawn/end points. Using fallback.");
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

            Debug.LogWarning("Could not find suitable spawn/end within chosenRegion at required distance. Using fallback.");
            spawnPoint = chosenRegion[0];
            endPoint = chosenRegion[chosenRegion.Count - 1];
        }

        // ===================== 新增/修改部分 ======================

        /// <summary>
        /// 建立Biome -> 可走地块列表 的字典，避免重复过滤
        /// </summary>
        void BuildBiomeTilesDictionary()
        {
            _biomeTilesDict = new Dictionary<Biome, List<Vector2Int>>();

            // 在 chosenRegion 中挑选每个位置，看对应的 Biome
            // 也可遍历全图，这里选择只遍历 chosenRegion
            foreach (var pos in chosenRegion)
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

        /// <summary>
        /// 资源放置逻辑：使用噪声 + 分桶 + 距离检查
        /// </summary>
        void PlaceResourcesFromBiomes()
        {
            if (chosenRegion == null || chosenRegion.Count == 0) return;
            if (_biomeTilesDict == null) return;

            foreach (var b in biomes)
            {
                if (b.biomeResource == null || b.numberOfResources <= 0) 
                    continue;
                if (!_biomeTilesDict.ContainsKey(b)) 
                    continue;

                // 取出该Biome可走位置
                var candidateTiles = new List<Vector2Int>(_biomeTilesDict[b]);
                // 打乱
                Shuffle(candidateTiles);

                int placedCount = 0;
                List<Vector2Int> placedPositions = new List<Vector2Int>();

                // 遍历 candidateTiles
                foreach (var tilePos in candidateTiles)
                {
                    if (placedCount >= b.numberOfResources)
                        break;

                    // 用噪声判定，这里仅做示例
                    float noiseVal = Mathf.PerlinNoise(
                        tilePos.x * b.resourceNoiseScale, 
                        tilePos.y * b.resourceNoiseScale);

                    // 如果噪声太小，就跳过
                    if (noiseVal < 0.5f) 
                        continue;

                    // 结合resourceDensity做随机
                    if (Random.value > b.resourceDensity)
                        continue;

                    // 检查与已放置资源的最小距离
                    if (!IsTooCloseToAny(tilePos, placedPositions, b.minResourceDistance))
                    {
                        // 决定放置哪种资源(从 BiomeResource 中选一个)
                        var chosenResource = PickResourceFromBiome(b);
                        if (chosenResource != null && chosenResource.resourcePrefab != null)
                        {
                            // 获取Prefab
                            GameObject prefab = b.biomeResource.GetRandomResourcePrefab();
                            if (prefab != null)
                            {
                                Vector3 wPos = baseTilemap.CellToWorld(new Vector3Int(tilePos.x, tilePos.y, 0)) 
                                               + new Vector3(0.5f, 0.5f, 0f);
                                Instantiate(prefab, wPos, Quaternion.identity);

                                placedPositions.Add(tilePos);
                                placedCount++;
                            }
                        }
                    }
                }
            }
        }

        // ===================== 工具方法 ======================

        /// <summary>
        /// 随机打乱List
        /// </summary>
        void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int rand = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[rand];
                list[rand] = temp;
            }
        }

        /// <summary>
        /// 从Biome的资源表中随机选一个资源条目 (示例)
        /// </summary>
        BiomeResource.BiomeResourceData PickResourceFromBiome(Biome b)
        {
            var table = b.biomeResource.GetResources();
            if (table == null) return null;

            var resList = new List<BiomeResource.BiomeResourceData>(table);
            if (resList.Count == 0) return null;

            int idx = Random.Range(0, resList.Count);
            return resList[idx];
        }

        /// <summary>
        /// 判断 tilePos 与 placedPositions 中任意点距离是否小于 minDist
        /// </summary>
        bool IsTooCloseToAny(Vector2Int tilePos, List<Vector2Int> placedPositions, float minDist)
        {
            foreach (var pp in placedPositions)
            {
                if (Vector2Int.Distance(pp, tilePos) < minDist)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// 判断某坐标是否可走 (也可直接查 _walkableArea[x,y])
        /// </summary>
        bool IsWalkable(int x, int y)
        {
            return _walkableArea[x,y];
        }
    }
}