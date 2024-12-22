using System.Collections.Generic;
using UnityEngine;
using _Script.Map.Procedural;
using _Script.Map.Procedural.BiomeData;
using UnityEngine.Tilemaps;

namespace _Script.Map.Generators
{
    /// <summary>
    /// 专门放置资源（例如矿石、树木资源、草药等）的生成逻辑
    /// </summary>
    public class ResourceGenerator
    {
        /// <summary>
        /// 将资源放置到场景中（GameObject 实例化等）
        /// </summary>
        /// <param name="biomes">全部的 Biome</param>
        /// <param name="biomeTilesDict">Biome->List<Vector2Int> 关系</param>
        /// <param name="chosenRegion">最大的可行走区域</param>
        /// <param name="baseTilemap">用于将坐标转换为世界坐标</param>
        public void PlaceResourcesFromBiomes(
            Biome[] biomes,
            Dictionary<Biome, List<Vector2Int>> biomeTilesDict,
            List<Vector2Int> chosenRegion,
            Tilemap baseTilemap)
        {
            if (chosenRegion == null || chosenRegion.Count == 0) return;
            if (biomeTilesDict == null) return;

            foreach (var b in biomes)
            {
                if (b.biomeResource == null || b.numberOfResources <= 0)
                    continue;
                if (!biomeTilesDict.ContainsKey(b))
                    continue;

                // 取出该Biome可走位置
                var candidateTiles = new List<Vector2Int>(biomeTilesDict[b]);
                // 打乱
                Shuffle(candidateTiles);

                int placedCount = 0;
                List<Vector2Int> placedPositions = new List<Vector2Int>();

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
                                Object.Instantiate(prefab, wPos, Quaternion.identity);

                                placedPositions.Add(tilePos);
                                placedCount++;
                            }
                        }
                    }
                }
            }
        }

        // 工具方法：打乱
        private static void Shuffle<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int rand = Random.Range(i, list.Count);
                (list[i], list[rand]) = (list[rand], list[i]);
            }
        }

        // 工具方法：从Biome的资源表中随机选一个资源条目 (示例)
        private BiomeResource.BiomeResourceData PickResourceFromBiome(Biome b)
        {
            var table = b.biomeResource.GetResources();
            if (table == null) return null;

            var resList = new List<BiomeResource.BiomeResourceData>(table);
            if (resList.Count == 0) return null;

            int idx = Random.Range(0, resList.Count);
            return resList[idx];
        }

        // 工具方法：判断 tilePos 与 placedPositions 中任意点距离是否小于 minDist
        private bool IsTooCloseToAny(Vector2Int tilePos, List<Vector2Int> placedPositions, float minDist)
        {
            foreach (var pp in placedPositions)
            {
                if (Vector2Int.Distance(pp, tilePos) < minDist)
                    return true;
            }
            return false;
        }
    }
}
