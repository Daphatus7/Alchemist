// Author : Peiyu Wang @ Daphatus
// 06 12 2024 12 31

// BiomeData.cs
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BiomeData", menuName = "MapGeneration/BiomeData")]
public class BiomeData : ScriptableObject
{
    [Header("Biome Identification")]
    public string biomeName;
    public Color gizmoColor = Color.green; // 用于在编辑器可视化

    [Header("Condition Thresholds")]
    // 通过高度、湿度等判断是否属于此生物群系
    public float minHeight;
    public float maxHeight;
    public float minMoisture;
    public float maxMoisture;

    [Header("Tiles")]
    public TileBase baseTile;
    public TileBase[] detailTiles; // 可选，用于增加随机细节
    public float detailChance = 0.1f; // 细节Tile出现概率

    [Header("Flora and Fauna")]
    public GameObject[] floraPrefabs; // 植物预制件
    public float floraSpawnChance = 0.05f; // 在该Biome上刷新植物的概率

    // 你可以添加更多字段，如资源分布、怪物、矿脉等
}
