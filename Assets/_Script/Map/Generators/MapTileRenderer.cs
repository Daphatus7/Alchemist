using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

// Namespaces for your own code
using _Script.Map.Procedural;
using _Script.Map.Tile.Tile_Base;
using _Script.Utilities;

namespace _Script.Map.Generators
{
    /// <summary>
    /// 仅负责将逻辑层生成的数据，渲染到实际 Tilemap 上。
    /// 并可选择在 Scene 中显示调试文字和网格线。
    /// </summary>
    public class MapTileRenderer
    {
        private readonly Tilemap _baseTilemap;
        private readonly Tilemap _obstaclesTilemap;
        private readonly Tilemap _floraTilemap;

        // Example: dictionary mapping each TileType -> array of TileBase variants
        private readonly Dictionary<TileType, TileBase[]> _tileSets;

        // If you want adjacency-based corners/edges:
        // Key: (TileState, TileState, TileState, TileState) => Value: TileBase
        private readonly Dictionary<Tuple<TileState, TileState, TileState, TileState>, TileBase> _neighbourTupleToTile;

        // 2D array for debug text display
        private TextMesh[,] _debugTextArray;

        public MapTileRenderer(
            Tilemap baseTilemap,
            Tilemap obstaclesTilemap,
            Tilemap floraTilemap,
            Dictionary<TileType, TileBase[]> tileSets
        )
        {
            _baseTilemap      = baseTilemap;
            _obstaclesTilemap = obstaclesTilemap;
            _floraTilemap     = floraTilemap;
            _tileSets         = tileSets;

            // Example adjacency dictionary (you can adapt):
            // "Surface" corresponds to "Grass", "Ground" to "Dirt".
            // The indices (like [0], [1], [6], etc.) refer to whichever tile arrangement you have.
            _neighbourTupleToTile = new Dictionary<Tuple<TileState, TileState, TileState, TileState>, TileBase>
            {
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Surface, TileState.Surface),
                    _tileSets[TileType.Grass][6]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Ground, TileState.Surface),
                    _tileSets[TileType.Grass][13]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Surface, TileState.Ground),
                    _tileSets[TileType.Grass][0]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Ground, TileState.Ground),
                    _tileSets[TileType.Grass][8]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Ground, TileState.Ground),
                    _tileSets[TileType.Grass][15]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Ground, TileState.Surface),
                    _tileSets[TileType.Grass][1]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Surface, TileState.Ground),
                    _tileSets[TileType.Grass][11]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Surface, TileState.Surface),
                    _tileSets[TileType.Grass][3]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Ground, TileState.Ground),
                    _tileSets[TileType.Grass][9]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Surface, TileState.Surface),
                    _tileSets[TileType.Grass][5]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Surface, TileState.Surface),
                    _tileSets[TileType.Grass][2]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Ground, TileState.Surface),
                    _tileSets[TileType.Grass][10]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Surface, TileState.Ground),
                    _tileSets[TileType.Grass][7]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Surface, TileState.Ground),
                    _tileSets[TileType.Grass][14]
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Ground, TileState.Surface),
                    _tileSets[TileType.Grass][4]
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Ground, TileState.Ground),
                    _tileSets[TileType.Grass][12]
                },
            };
        }

        /// <summary>
        /// 绘制最终地图，并可在 Scene 中显示网格线和每个单元格的文字。
        /// </summary>
        /// <param name="mapTiles">MapTileLogic 生成的最终地图数据</param>
        /// <param name="tileSet">与本渲染器对应的 TileBase 数组</param>
        /// <param name="debug">是否在 Scene 中调试显示网格线 & 文本</param>
        public void RenderFinalMap(MapTileLogic mapTiles, Dictionary<TileType, TileBase[]> tileSet, bool debug = true)
        {
            // 清理旧 Tile
            _baseTilemap.ClearAllTiles();
            _obstaclesTilemap.ClearAllTiles();
            _floraTilemap.ClearAllTiles();

            int width  = mapTiles.MapTiles.GetLength(0);
            int height = mapTiles.MapTiles.GetLength(1);

            // 如果要调试，我们就创建一个同尺寸的 TextMesh 数组
            if (debug)
            {
                _debugTextArray = new TextMesh[width, height];
            }

            // 遍历整个地图
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    var tileData = mapTiles.MapTiles[x, y];
                    if (tileData.TileType == TileType.None)
                        continue;

                    // 根据可行走区域决定是 baseTile 还是 obstaclesTile
                    var cellPos = new Vector3Int(x, y, 0);
                    TileBase finalTile = GetDisplayTile(cellPos); // adjacency-based or fallback

                    if (mapTiles.WalkableArea[x, y])
                    {
                        _baseTilemap.SetTile(cellPos, finalTile);
                    }
                    else
                    {
                        _obstaclesTilemap.SetTile(cellPos, finalTile);
                    }

                    // ------ Debug Lines & Text ------
                    if (true)
                    {
                        // 用白线绘制每个单元格的边框
                        Vector3 worldPos = new Vector3(x, y, 0);
                        // Lines going "up" and "right"
                        Debug.DrawLine(worldPos, worldPos + new Vector3(0, 1), Color.white, 100f);
                        Debug.DrawLine(worldPos, worldPos + new Vector3(1, 0), Color.white, 100f);

                        // 在单元格中央创建一段文字
                        // 你可以显示 tileType.ToString() 或其他信息
                        string debugString = tileData.TileType.ToString();
                        Vector3 textPos = worldPos + new Vector3(0.5f, 0.5f, 0);

                        _debugTextArray[x, y] = Helper.CreateWorldText(
                            debugString,
                            null,         // optional parent transform
                            textPos,
                            20,           // font size
                            Color.white,
                            TextAnchor.MiddleCenter
                        );
                    }
                }
            }

            // 绘制最外面一条线
            if (debug)
            {
                // Top boundary
                Debug.DrawLine(new Vector3(0, height, 0), new Vector3(width, height, 0), Color.white, 100f);
                // Right boundary
                Debug.DrawLine(new Vector3(width, 0, 0), new Vector3(width, height, 0), Color.white, 100f);
            }
        }

        /// <summary>
        /// 返回一个 TileBase，用于展示四方向/双方向等连接逻辑。
        /// </summary>
        private TileBase GetDisplayTile(Vector3Int coords)
        {
            // This is where you'd do adjacency checks if you want
            // 1) Convert each neighbor cell to TileState (Surface/Ground)
            // 2) Build a tuple
            // 3) Look up in _neighbourTupleToTile

            // For demonstration, let's do a quick example:
            // We'll say "always pick index [0] of Grass if you don't do adjacency."
            // return _tileSets[TileType.Grass][0];

            // OR do something like:
            var tuple = CreateTileStateTuple(coords);
            if (_neighbourTupleToTile.TryGetValue(tuple, out TileBase tile))
            {
                return tile;
            }
            else
            {
                // fallback
                return _tileSets[TileType.Grass][0];
            }
        }

        /// <summary>
        /// 以 (topLeft, topRight, botLeft, botRight) 的顺序，构建一个 TileState 4元组。
        /// 你需要根据你实际项目中如何判定相邻格子而修改。
        /// </summary>
        private Tuple<TileState, TileState, TileState, TileState> CreateTileStateTuple(Vector3Int coords)
        {
            // For example, we define offsets:
            // topLeft => coords + (0,1)
            // topRight => coords + (1,1)
            // botLeft => coords
            // botRight => coords + (1,0)

            TileState topLeft  = GetTileState(coords + new Vector3Int(0, 1, 0));
            TileState topRight = GetTileState(coords + new Vector3Int(1, 1, 0));
            TileState botLeft  = GetTileState(coords);
            TileState botRight = GetTileState(coords + new Vector3Int(1, 0, 0));

            return Tuple.Create(topLeft, topRight, botLeft, botRight);
        }

        /// <summary>
        /// 示例：把坐标处的 TileType 转换成 (Surface / Ground)。
        /// 实际上你还要检查是否越界。
        /// </summary>
        private TileState GetTileState(Vector3Int coords)
        {
            // Just a placeholder for demonstration:
            // If out of bounds, pretend it's Ground
            // In a real scenario, you'd store a reference to MapTileLogic or
            // some other data source to read the actual tile at coords.
            // Then do: if tile == Grass => Surface, if tile == Dirt => Ground, etc.

            return TileState.Surface; 
        }
    }

    /// <summary>
    /// 用于区分相邻格子的类型(单纯演示)。Grass -> Surface, Dirt -> Ground
    /// </summary>

}