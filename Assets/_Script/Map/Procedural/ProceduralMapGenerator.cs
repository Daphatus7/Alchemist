using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// Procedural Map Generator for a 2D RPG-style Tilemap.
/// Attach this script to a GameObject with a Tilemap and TilemapRenderer.
/// Configure tile references in the inspector.
/// This example uses simple Perlin noise and basic generation techniques.
/// Extend and refine the logic as needed for your specific game.
/// </summary>
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

    [Header("Tilemap Reference")]
    public Tilemap baseTilemap;

    [Header("Noise Settings")]
    [Range(0.0f, 1.0f)] public float perlinScale = 0.1f;
    [Range(0, 100)] public int forestThreshold = 50;
    [Range(0, 100)] public int waterThreshold = 35;

    [Header("POI Settings")]
    public int numberOfPOIs = 3;
    public int minDistanceBetweenPOIs = 8;

    // Internal data structure for tiles
    private TileBase[,] mapTiles;

    // Data arrays for generation
    private bool[,] walkableArea; // For cellular automaton or other pathing generation

    // Use this for initialization
    void Start()
    {
        GenerateMap();
    }

    /// <summary>
    /// Main entry point for generating the procedural map.
    /// </summary>
    public void GenerateMap()
    {
        if (baseTilemap == null)
        {
            Debug.LogError("No Tilemap assigned. Please assign a Tilemap to 'baseTilemap' in the inspector.");
            return;
        }

        // Initialize arrays
        mapTiles = new TileBase[width, height];
        walkableArea = new bool[width, height];

        // Step 1: Initialize the map boundaries and fill with walls
        InitializeBoundary();

        // Step 2: Generate walkable areas (grass/dirt) using a simple cellular automaton or random fill
        GenerateWalkableArea();

        // Step 3: Add terrain features using Perlin Noise (water, forest)
        AddPerlinNoiseTerrain();

        // Step 4: Place Points of Interest (POIs)
        PlacePOIs();

        // Step 5: Finalize and cleanup unreachable tiles if necessary
        PostProcessMap();

        // Step 6: Render the final map to the Tilemap
        RenderMap();
    }

    /// <summary>
    /// Create an outer boundary of walls around the map.
    /// </summary>
    void InitializeBoundary()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Fill edges with walls
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                {
                    mapTiles[x, y] = wallTile;
                    walkableArea[x, y] = false;
                }
                else
                {
                    // Temporarily fill with grass or blank until processed
                    mapTiles[x, y] = null;
                }
            }
        }
    }

    /// <summary>
    /// Generate walkable areas inside the map. For simplicity, we randomly assign walkable spots,
    /// then use a cellular automaton to smooth out the area.
    /// </summary>
    void GenerateWalkableArea()
    {
        // Initial random fill
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                walkableArea[x, y] = Random.value > 0.3f; // 70% chance of being walkable initially
            }
        }

        // Cellular Automaton step (optional): Smooth out areas
        for (int i = 0; i < 3; i++)
        {
            walkableArea = SmoothWalkableArea(walkableArea);
        }

        // Assign tiles to walkable areas
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (walkableArea[x, y])
                {
                    // Default walkable tile: grass or dirt
                    mapTiles[x, y] = (Random.value > 0.5f) ? grassTile : dirtTile;
                }
                else
                {
                    // Non-walkable internal walls
                    mapTiles[x, y] = wallTile;
                }
            }
        }
    }

    /// <summary>
    /// Apply a cellular automaton rule to smooth the walkable regions.
    /// </summary>
    bool[,] SmoothWalkableArea(bool[,] area)
    {
        bool[,] newArea = new bool[width, height];
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                int neighborCount = CountWalkableNeighbors(area, x, y);
                // If more than 4 neighbors are walkable, this tile becomes walkable
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

    /// <summary>
    /// Use Perlin noise to distribute water and forest tiles, replacing some grass/dirt tiles.
    /// </summary>
    void AddPerlinNoiseTerrain()
    {
        float xOffset = Random.Range(0f, 1000f);
        float yOffset = Random.Range(0f, 1000f);

        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < height - 1; y++)
            {
                if (mapTiles[x, y] != null && mapTiles[x, y] != wallTile)
                {
                    float noiseValue = Mathf.PerlinNoise((x + xOffset) * perlinScale, (y + yOffset) * perlinScale) * 100f;

                    // Forest
                    if (noiseValue > forestThreshold)
                    {
                        mapTiles[x, y] = forestTile;
                        walkableArea[x, y] = false; // Forest is non-walkable
                    }
                    // Water
                    else if (noiseValue < waterThreshold)
                    {
                        mapTiles[x, y] = waterTile;
                        walkableArea[x, y] = false; // Water is non-walkable
                    }
                    // Otherwise remain grass/dirt (walkable)
                }
            }
        }
    }

    /// <summary>
    /// Place Points of Interest randomly, ensuring a minimum distance between them and placing suitable tiles.
    /// </summary>
    void PlacePOIs()
    {
        List<Vector2Int> placedPOIs = new List<Vector2Int>();

        int attempts = 0;
        int maxAttempts = numberOfPOIs * 50; // Arbitrary to avoid infinite loops

        while (placedPOIs.Count < numberOfPOIs && attempts < maxAttempts)
        {
            attempts++;
            int x = Random.Range(2, width - 2);
            int y = Random.Range(2, height - 2);

            // Must be on a walkable tile to place a POI
            if (mapTiles[x, y] == grassTile || mapTiles[x, y] == dirtTile)
            {
                // Check distance from other POIs
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
                    // Place POI
                    mapTiles[x, y] = poiTile;
                    placedPOIs.Add(new Vector2Int(x, y));

                    // Surround POI with a theme - for now just some walls around it
                    SurroundPOIWithTerrain(x, y);
                }
            }
        }
    }

    /// <summary>
    /// Example: Surround the POI with some walls or special tiles.
    /// Adjust logic for different POI themes.
    /// </summary>
    void SurroundPOIWithTerrain(int px, int py)
    {
        // For simplicity, just add a ring of walls around the POI,
        // except for the POI tile itself.
        for (int x = px - 1; x <= px + 1; x++)
        {
            for (int y = py - 1; y <= py + 1; y++)
            {
                if (!(x == px && y == py)) // leave POI tile intact
                {
                    // Place a non-walkable obstacle (like a rock wall) to highlight the POI
                    // Only if it's currently walkable or grass/dirt
                    if (mapTiles[x, y] != null && (mapTiles[x, y] == grassTile || mapTiles[x, y] == dirtTile))
                    {
                        mapTiles[x, y] = wallTile;
                        walkableArea[x, y] = false;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Remove isolated walkable tiles or unreachable zones if needed.
    /// For this example, we'll skip complex pathfinding steps.
    /// In a production scenario, run a flood-fill from known entrances and remove unreachable walkable tiles.
    /// </summary>
    void PostProcessMap()
    {
        // Optional: Implement logic to ensure navigability. 
        // For now, we do nothing.
    }

    /// <summary>
    /// Render the final map to the Tilemap component.
    /// </summary>
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
}
