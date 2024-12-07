// Author : Peiyu Wang @ Daphatus
// 08 12 2024 12 32

using UnityEngine;
using UnityEngine.Tilemaps;


namespace _Script.Map.Procedural
{
    public class BiomeMapGenerator : MonoBehaviour
    {
        [Header("Map Dimensions")]
        public int width = 100;
        public int height = 100;

        [Header("Tiles")]
        public TileBase wallTile;
        public TileBase grassTile;
        public TileBase forestTile;
        public TileBase waterTile;
        public TileBase coastTile;
        public TileBase mountainTile;
        public TileBase plainsFloraTile;
        public TileBase forestFloraTile;

        [Header("Tilemap References")]
        public Tilemap baseTilemap;
        public Tilemap floraTilemap;

        [Header("Biome Noise Settings")]
        [Range(0.0f, 1.0f)] public float heightNoiseScale = 0.02f;
        [Range(0.0f, 1.0f)] public float vegetationNoiseScale = 0.02f;
        public float waterLevel = 0.3f;
        public float forestThreshold = 0.6f;
        public float mountainThreshold = 0.8f;
        
        [Header("Rivers and Coastal")]
        public float riverChance = 0.02f; // Probability of generating a "river" tile in suitable zones
        public float coastBand = 0.05f;   // Band around water-level considered "coastal"

        [Header("Flora Settings")]
        public float floraDensity = 0.3f;
        public float floraNoiseScale = 0.05f;
        public float floraMinNoise = 0.4f;
        public float floraMaxNoise = 0.6f;

        [Header("Seed")]
        public int seed = 0;
        public bool useRandomSeed = false;

        // Internal data structure for tiles
        private TileBase[,] mapTiles;
        private bool[,] walkableArea; 

        void Start()
        {
            GenerateMap();
        }

        public void GenerateMap()
        {
            if (baseTilemap == null)
            {
                Debug.LogError("No Tilemap assigned. Please assign a Tilemap to 'baseTilemap'.");
                return;
            }

            if (floraTilemap == null)
            {
                Debug.LogError("No Flora Tilemap assigned. Please assign a Tilemap to 'floraTilemap'.");
                return;
            }

            // Initialize random seed
            if (!useRandomSeed)
                Random.InitState(seed);
            else
                Random.InitState(System.DateTime.Now.GetHashCode());

            mapTiles = new TileBase[width, height];
            walkableArea = new bool[width, height];

            // Generate noise offsets for height and vegetation
            float heightOffsetX = Random.Range(0f, 10000f);
            float heightOffsetY = Random.Range(0f, 10000f);
            float vegOffsetX = Random.Range(0f, 10000f);
            float vegOffsetY = Random.Range(0f, 10000f);

            InitializeBoundary();
            AssignBiomes(heightOffsetX, heightOffsetY, vegOffsetX, vegOffsetY);
            PlaceRivers(heightOffsetX, heightOffsetY);
            PostProcessMap();
            RenderMap();

            PlaceBiomeFlora();
        }

        void InitializeBoundary()
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        mapTiles[x, y] = wallTile;
                        walkableArea[x, y] = false;
                    }
                    else
                    {
                        mapTiles[x, y] = null;
                    }
                }
            }
        }

        /// <summary>
        /// Assign biomes based on perlin noise height and vegetation.
        /// Height decides water, coast, plains, mountains.
        /// Vegetation decides if plains become forest.
        /// </summary>
        void AssignBiomes(float hx, float hy, float vx, float vy)
        {
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    float hxVal = Mathf.PerlinNoise((x + hx) * heightNoiseScale, (y + hy) * heightNoiseScale);
                    float vxVal = Mathf.PerlinNoise((x + vx) * vegetationNoiseScale, (y + vy) * vegetationNoiseScale);

                    if (hxVal < waterLevel)
                    {
                        // Water biome
                        mapTiles[x, y] = waterTile;
                        walkableArea[x, y] = false;
                    }
                    else
                    {
                        // Land biome - determine type
                        bool nearWater = (hxVal < waterLevel + coastBand);
                        if (nearWater)
                        {
                            // Coastal transition tiles
                            mapTiles[x, y] = coastTile;
                            walkableArea[x, y] = true;
                        }
                        else
                        {
                            if (hxVal > mountainThreshold)
                            {
                                // Mountains
                                mapTiles[x, y] = mountainTile;
                                walkableArea[x, y] = false;
                            }
                            else
                            {
                                // Plains or forest depending on vegetation
                                if (vxVal > forestThreshold)
                                {
                                    mapTiles[x, y] = forestTile;
                                    walkableArea[x, y] = false; // Forest tiles non-walkable
                                }
                                else
                                {
                                    mapTiles[x, y] = grassTile;
                                    walkableArea[x, y] = true;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Very simple approach to "rivers" by adding river tiles along lower-height (but not water) areas.
        /// Adjust as needed. For a more natural river, you'd implement path carving or noise-based line drawing.
        /// </summary>
        void PlaceRivers(float hx, float hy)
        {
            for (int x = 2; x < width - 2; x++)
            {
                for (int y = 2; y < height - 2; y++)
                {
                    // Only place rivers on grass tiles
                    if (mapTiles[x, y] == grassTile && Random.value < riverChance)
                    {
                        // Turn this tile into a water tile to simulate a river cell
                        mapTiles[x, y] = waterTile;
                        walkableArea[x, y] = false;
                    }
                }
            }
        }

        /// <summary>
        /// Optional: Implement logic to ensure connectivity or remove isolated tiles.
        /// In this example, we skip advanced pathfinding steps.
        /// </summary>
        void PostProcessMap()
        {
            // Could implement smoothing or path checks here.
        }

        void RenderMap()
        {
            baseTilemap.ClearAllTiles();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mapTiles[x, y] != null)
                    {
                        baseTilemap.SetTile(new Vector3Int(x, y, 0), mapTiles[x, y]);
                    }
                }
            }
        }

        /// <summary>
        /// Place flora according to the biome. For example:
        /// - Plains: random flowers or tall grass
        /// - Forest edges: mushrooms or bushes
        /// Flora placement is influenced by Perlin noise for scattered clusters.
        /// </summary>
        void PlaceBiomeFlora()
        {
            floraTilemap.ClearAllTiles();

            if (plainsFloraTile == null && forestFloraTile == null) return;

            float floraXOffset = Random.Range(0f, 1000f);
            float floraYOffset = Random.Range(0f, 1000f);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    TileBase currentTile = mapTiles[x, y];

                    // Consider placing flora only on certain biome tiles
                    if (currentTile == grassTile || currentTile == forestTile)
                    {
                        float noiseValue = Mathf.PerlinNoise((x + floraXOffset) * floraNoiseScale, 
                                                             (y + floraYOffset) * floraNoiseScale);

                        if (noiseValue >= floraMinNoise && noiseValue <= floraMaxNoise && Random.value < floraDensity)
                        {
                            // If on forest tile, place forest flora (e.g. mushrooms)
                            // If on grass tile, place plains flora (e.g. flowers)
                            if (currentTile == forestTile && forestFloraTile != null)
                            {
                                floraTilemap.SetTile(new Vector3Int(x, y, 0), forestFloraTile);
                            }
                            else if (currentTile == grassTile && plainsFloraTile != null)
                            {
                                floraTilemap.SetTile(new Vector3Int(x, y, 0), plainsFloraTile);
                            }
                        }
                    }
                }
            }
        }
    }
}