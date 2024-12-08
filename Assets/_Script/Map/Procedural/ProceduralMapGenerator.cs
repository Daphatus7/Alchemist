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

        // A list of valid, walkable, obstacle-free tiles for spawning
        private List<Vector2Int> validSpawnTiles;

        void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            if (baseTilemap == null || floraTilemap == null || obstaclesTilemap == null)
            {
                Debug.LogError("Assign baseTilemap, floraTilemap, and obstaclesTilemap.");
                return;
            }

            if (biomes == null || biomes.Length == 0)
            {
                Debug.LogError("No biomes defined. Please assign biomes.");
                return;
            }

            if (!useRandomSeed)
                Random.InitState(seed);
            else
                Random.InitState(System.DateTime.Now.GetHashCode());

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

            // Sync walkability after all changes
            SyncWalkableAreaWithFinalMap();
            // Build a list of all valid walkable spawn tiles
            BuildValidSpawnTilesList();

            PlaceMonstersFromBiomes();
            PlaceResourcesFromBiomes();
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

        Biome PickBiomeByNoise(float n)
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
                        _walkableArea[x,y] = true;
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

        void BuildValidSpawnTilesList()
        {
            validSpawnTiles = new List<Vector2Int>();
            for (int x=1;x<width-1;x++)
            {
                for (int y=1;y<height-1;y++)
                {
                    if(_walkableArea[x,y] && obstaclesTilemap.GetTile(new Vector3Int(x,y,0))==null)
                    {
                        // This tile can host monsters/resources
                        validSpawnTiles.Add(new Vector2Int(x,y));
                    }
                }
            }
        }

        void PlaceMonstersFromBiomes()
        {
            // Instead of random attempts, pick from validSpawnTiles
            foreach(var b in biomes)
            {
                if(b.monsterPrefab==null||b.numberOfMonsters<=0) continue;

                // Shuffle validSpawnTiles or pick random indices
                // Using a simple approach:
                var candidateList = new List<Vector2Int>(validSpawnTiles);
                int attempts=0;

                // Place monsters ensuring min distance
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
                        // Remove this tile from candidateList to avoid spawning another monster here
                        candidateList.RemoveAt(randIndex);
                    }
                    else
                    {
                        // Just remove this candidate from the list to avoid repeated checking
                        candidateList.RemoveAt(randIndex);
                    }
                }
            }
        }

        void PlaceResourcesFromBiomes()
        {
            foreach(var b in biomes)
            {
                if(b.resourcePrefab==null||b.numberOfResources<=0) continue;

                var candidateList = new List<Vector2Int>(validSpawnTiles);
                List<Vector2Int> placedR = new List<Vector2Int>();
                int attempts=0;

                while(placedR.Count<b.numberOfResources && candidateList.Count>0 && attempts<b.numberOfResources*100)
                {
                    attempts++;
                    int randIndex=Random.Range(0,candidateList.Count);
                    Vector2Int spot=candidateList[randIndex];

                    // Distance check
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
                        candidateList.RemoveAt(randIndex);
                    }
                    else
                    {
                        candidateList.RemoveAt(randIndex);
                    }
                }
            }
        }
    }
}
