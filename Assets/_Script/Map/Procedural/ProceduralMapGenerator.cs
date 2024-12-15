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

        [Header("Base Tilemap References")]
        public Tilemap baseTilemap;
        public Tilemap obstaclesTilemap;
        public Tilemap floraTilemap;

        [Header("Debug")]
        public Tilemap debugTilemap;   // Assign in inspector for debugging
        public TileBase debugTile;     // A simple white tile to tint with color
        
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

        // Instead of just one big list, we now have regions of reachable tiles
        private List<List<Vector2Int>> reachableAreas;
        // We'll choose tiles from the largest reachable area for spawn, end, monsters, etc.
        private List<Vector2Int> chosenRegion;

        private Vector2Int _spawnPoint; public Vector2Int SpawnPoint => _spawnPoint;
        private Vector2Int _endPoint; public Vector2Int EndPoint => _endPoint;
        [SerializeField] private int _minDistance = 10;

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
            PlaceResourcesFromBiomes();

            GenerateSpawnAndEndPoint(_minDistance, out Vector2Int spawnPoint, out Vector2Int endPoint);
            _spawnPoint = spawnPoint;
            sPoint = spawnPoint;
            _endPoint = endPoint;
            ePoint = endPoint;
            
            
            // DebugPrintReachableAreas();
            // HighlightReachableAreas();
            // DebugHighlightChosenRegion();
            // DebugMarkSpawnEndPoints();
            return true;
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
                    float n = Mathf.PerlinNoise((x+xOffset)*biomeNoiseScale,(y+yOffset)*biomeNoiseScale);
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

                    float val = Mathf.PerlinNoise((x+xOffset)*perlinScale,(y+yOffset)*perlinScale)*100f;
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
                // No reachable areas? fallback
                return new List<Vector2Int>();
            }

            // Pick largest by tile count
            List<Vector2Int> largest = reachableAreas[0];
            foreach(var area in reachableAreas)
            {
                if(area.Count>largest.Count)
                    largest=area;
            }
            return largest;
        }

        /// <summary>
        /// Print information about the reachable areas to the console.
        /// </summary>
        void DebugPrintReachableAreas()
        {
            if (reachableAreas == null || reachableAreas.Count == 0)
            {
                Debug.Log("No reachable areas found.");
                return;
            }

            Debug.Log($"Number of reachable areas: {reachableAreas.Count}");
            for (int i = 0; i < reachableAreas.Count; i++)
            {
                Debug.Log($"Area {i}: Size = {reachableAreas[i].Count} tiles");
            }
        }

        /// <summary>
        /// Highlights each reachable area with a different color to visualize their locations.
        /// </summary>
        void HighlightReachableAreas()
        {
            if (debugTilemap == null || debugTile == null)
            {
                Debug.LogWarning("Debug Tilemap or Debug Tile not assigned, cannot highlight areas.");
                return;
            }

            Color[] colors = new Color[] {
                Color.red, Color.green, Color.blue,
                Color.magenta, Color.cyan, Color.yellow,
                Color.gray, new Color(1f,0.5f,0f), new Color(0.5f,0f,1f)
            };

            for (int i = 0; i < reachableAreas.Count; i++)
            {
                var area = reachableAreas[i];
                Color c = (i < colors.Length) ? colors[i] : new Color(Random.value, Random.value, Random.value);

                foreach (var tilePos in area)
                {
                    Vector3Int cellPos = new Vector3Int(tilePos.x, tilePos.y, 0);
                    debugTilemap.SetTile(cellPos, debugTile);
                    debugTilemap.SetColor(cellPos, c);
                }
            }
        }

        /// <summary>
        /// Highlights the chosen (largest) reachable region with a distinct border or color.
        /// </summary>
        void DebugHighlightChosenRegion()
        {
            if (debugTilemap == null || debugTile == null || chosenRegion == null || chosenRegion.Count == 0)
                return;

            // Use white color for chosen region as an overlay
            Color chosenColor = Color.white;

            foreach (var tilePos in chosenRegion)
            {
                Vector3Int cellPos = new Vector3Int(tilePos.x, tilePos.y, 0);
                if (debugTilemap.GetTile(cellPos) != null)
                {
                    // Mix chosenColor with existing color?
                    // For simplicity, just overwrite with chosenColor for clarity
                    debugTilemap.SetColor(cellPos, chosenColor);
                }
                else
                {
                    debugTilemap.SetTile(cellPos, debugTile);
                    debugTilemap.SetColor(cellPos, chosenColor);
                }
            }
        }

        /// <summary>
        /// Mark the spawn and end points with a distinct color or tile on the debug tilemap.
        /// </summary>
        void DebugMarkSpawnEndPoints()
        {
            if (debugTilemap == null || debugTile == null)
                return;

            // Spawn point as green, end point as red
            if (_spawnPoint != Vector2Int.zero)
            {
                Vector3Int spawnCell = new Vector3Int(_spawnPoint.x, _spawnPoint.y, 0);
                debugTilemap.SetTile(spawnCell, debugTile);
                debugTilemap.SetColor(spawnCell, Color.green);
            }

            if (_endPoint != Vector2Int.zero)
            {
                Vector3Int endCell = new Vector3Int(_endPoint.x, _endPoint.y, 0);
                debugTilemap.SetTile(endCell, debugTile);
                debugTilemap.SetColor(endCell, Color.red);
            }
        }
        
        void PlaceMonstersFromBiomes()
        {
            // Now we choose from chosenRegion instead of validSpawnTiles
            // chosenRegion is guaranteed to be walkable and reachable
            if(chosenRegion==null || chosenRegion.Count==0) return;

            foreach(var b in biomes)
            {
                if(b.monsterPrefab==null||b.numberOfMonsters<=0) continue;

                var candidateList = new List<Vector2Int>(chosenRegion);
                int attempts=0;
                List<Vector2> placedM = new List<Vector2>();

                while(placedM.Count<b.numberOfMonsters && candidateList.Count>0 && attempts< b.numberOfMonsters*100)
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
            // Ensure chosenRegion is not empty and has at least 2 tiles
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
                // Pick a random spawn candidate from chosenRegion
                Vector2Int candidateSpawn = chosenRegion[Random.Range(0, chosenRegion.Count)];

                // Find tiles at least minimumDistance away within chosenRegion
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
                    // Pick a random end tile from farEnoughTiles
                    Vector2Int candidateEnd = farEnoughTiles[Random.Range(0, farEnoughTiles.Count)];
                    spawnPoint = candidateSpawn;
                    endPoint = candidateEnd;
                    return;
                }
            }

            // If no suitable pair found after maxAttempts, fallback
            Debug.LogWarning("Could not find suitable spawn/end within chosenRegion at required distance. Using fallback.");
            spawnPoint = chosenRegion[0];
            endPoint = chosenRegion[chosenRegion.Count - 1];
        }

        void PlaceResourcesFromBiomes()
        {
            if(chosenRegion==null || chosenRegion.Count==0) return;

            foreach(var b in biomes)
            {
                if(b.resourcePrefab==null||b.numberOfResources<=0) continue;

                var candidateList = new List<Vector2Int>(chosenRegion);
                List<Vector2Int> placedR = new List<Vector2Int>();
                int attempts=0;

                while(placedR.Count<b.numberOfResources && candidateList.Count>0 && attempts<b.numberOfResources*100)
                {
                    attempts++;
                    int randIndex=Random.Range(0,candidateList.Count);
                    Vector2Int spot=candidateList[randIndex];

                    bool tooClose=false;
                    foreach(var rPos in placedR)
                    {
                        if(Vector2Int.Distance(rPos,spot)<b.minResourceDistance)
                        {
                            tooClose=true;
                            break;
                        }
                    }

                    if(!tooClose && Random.value<b.resourceDensity)
                    {
                        Vector3 wPos=baseTilemap.CellToWorld(new Vector3Int(spot.x,spot.y,0));
                        Instantiate(b.resourcePrefab,wPos+new Vector3(0.5f,0.5f,0f),Quaternion.identity);
                        placedR.Add(spot);
                    }

                    candidateList.RemoveAt(randIndex);
                }
            }
        }
    }
}
