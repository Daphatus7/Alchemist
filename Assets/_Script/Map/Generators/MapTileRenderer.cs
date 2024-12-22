using _Script.Map.Procedural;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Generators
{
    /// <summary>
    /// 仅负责将逻辑层生成的数据，渲染到实际 Tilemap 上。
    /// </summary>
    public class MapTileRenderer
    {
        private readonly Tilemap _baseTilemap;
        private readonly Tilemap _obstaclesTilemap;
        private readonly Tilemap _floraTilemap;

        public MapTileRenderer(Tilemap baseTilemap, Tilemap obstaclesTilemap, Tilemap floraTilemap)
        {
            _baseTilemap = baseTilemap;
            _obstaclesTilemap = obstaclesTilemap;
            _floraTilemap = floraTilemap;
        }

        /// <summary>
        /// 根据最终的 mapTiles / obstacleTiles / walkableArea 来渲染地图
        /// </summary>
        public void RenderFinalMap(TileBase[,] mapTiles, TileBase[,] obstacleTiles, bool[,] walkableArea)
        {
            // 先清理
            _baseTilemap.ClearAllTiles();
            _obstaclesTilemap.ClearAllTiles();
            _floraTilemap.ClearAllTiles();

            int width = mapTiles.GetLength(0);
            int height = mapTiles.GetLength(1);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileBase ground = mapTiles[x, y];
                    if (ground == null) continue;

                    if (walkableArea[x, y])
                    {
                        _baseTilemap.SetTile(new Vector3Int(x, y, 0), ground);
                    }
                    else
                    {
                        TileBase obstacle = obstacleTiles[x, y] != null ? obstacleTiles[x, y] : ground;
                        _obstaclesTilemap.SetTile(new Vector3Int(x, y, 0), obstacle);
                    }
                }
            }
        }

        /// <summary>
        /// 根据 Biome 再放置一些 flora Tile 到 floraTilemap
        /// </summary>
        public void PlaceFlora(TileBase[,] mapTiles, bool[,] walkableArea, Biome[,] tileBiomes)
        {
            int width = mapTiles.GetLength(0);
            int height = mapTiles.GetLength(1);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    Biome b = tileBiomes[x, y];
                    if (b == null || b.floraTile == null) 
                        continue;

                    if (walkableArea[x, y] && _obstaclesTilemap.GetTile(new Vector3Int(x, y, 0)) == null)
                    {
                        float fXOff = Random.Range(0f, 1000f);
                        float fYOff = Random.Range(0f, 1000f);
                        float noise = Mathf.PerlinNoise(
                            (x + fXOff) * b.floraNoiseScale,
                            (y + fYOff) * b.floraNoiseScale);

                        if (noise >= b.floraMinNoise && noise <= b.floraMaxNoise && 
                            Random.value < b.floraDensity)
                        {
                            _floraTilemap.SetTile(new Vector3Int(x, y, 0), b.floraTile);
                        }
                    }
                }
            }
        }
    }
}
