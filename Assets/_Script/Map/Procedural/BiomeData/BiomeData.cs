// Author : Peiyu Wang @ Daphatus
// 06 12 2024 12 31

using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "BiomeData", menuName = "MapGeneration/BiomeData")]
public class BiomeData : ScriptableObject
{
    [Header("Biome Identification")]
    public string biomeName;
    public Color gizmoColor = Color.green;

    [Header("Condition Thresholds")]
    public float minHeight;
    public float maxHeight;
    public float minMoisture;
    public float maxMoisture;

    [Header("Tiles")]
    public TileBase baseTile;
    public TileBase[] detailTiles;
    public float detailChance = 0.1f;

    [Header("Flora and Fauna")]
    public GameObject[] floraPrefabs;
    public float floraSpawnChance = 0.05f;
}
