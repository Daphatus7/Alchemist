using _Script.Map.Procedural.BiomeData;
using _Script.Map.Tile.Tile_Base;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Procedural
{
    [CreateAssetMenu(fileName = "NewBiome", menuName = "Map/Biome")]
    public class Biome : ScriptableObject
    {
        [BoxGroup("Basic Tiles")]
        public TileType mainGroundTile; // main ground tile for this biome
        public TileType forestTile;
        public TileType waterTile;
        public TileType wallTile;
        public TileType grassTile;
        public TileType dirtTile;

        [BoxGroup("Thresholds & Noise")]
        [Range(0f, 1f)] 
        public float selectionThreshold; 
        // 用于判断当前格子是否属于此Biome的阈值

        [Range(0, 100)]
        public int forestThreshold = 50;
        [Range(0, 100)]
        public int waterThreshold = 35;

        [BoxGroup("Flora Settings")]
        public TileBase floraTile;       // 这里依然是 TileBase 用于实际渲染
        [Range(0f, 1f)] public float floraDensity = 0.3f;
        [Range(0f, 1f)] public float floraNoiseScale = 0.05f;
        public float floraMinNoise = 0.4f;
        public float floraMaxNoise = 0.6f;

        [BoxGroup("Resource Settings")]
        public BiomeResource biomeResource;
        [Range(0f, 1f)] public float resourceDensity = 0.2f;
        [Range(0f, 1f)] public float resourceNoiseScale = 0.05f;  
        
        public GameObject resourcePrefab;    // 示例资源Prefab
        public int numberOfResources = 10;   // 该Biome要放多少个资源
        public float minResourceDistance = 3f;

        [BoxGroup("Monster Settings")]
        public GameObject monsterPrefab;
        public int numberOfMonsters = 5;
        public float minMonsterDistance = 10f;

        [BoxGroup("POI Settings")]
        public TileType poiTile;
        public int numberOfPOIs = 3;
        public int minDistanceBetweenPOIs = 8;

        [BoxGroup("Rocks Settings")]
        public TileType rockTile;
        [Range(0f,1f)] public float rockDensity = 0.2f;
        [Range(0f,1f)] public float rockNoiseScale = 0.05f;
        public float rockMinNoise = 0.5f;
        public float rockMaxNoise = 0.8f;
    }
}