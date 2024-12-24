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
        private readonly MapTileLogic _mapLogic;
        // Example: dictionary mapping each TileType -> array of TileBase variants
        private readonly Dictionary<TileType, TileBase[]> _tileSets;

        // If you want adjacency-based corners/edges:
        // Key: (TileState, TileState, TileState, TileState) => Value: TileBase
        private readonly Dictionary<Tuple<TileState, TileState, TileState, TileState>, int> _neighbourTupleToTile;

        // 2D array for debug text display
        private TextMesh[,] _debugTextArray;

        public MapTileRenderer(
            Tilemap baseTilemap,
            Tilemap obstaclesTilemap,
            Tilemap floraTilemap,
            Dictionary<TileType, TileBase[]> tileSets,
            MapTileLogic mapTiles
        )
        {
            _baseTilemap      = baseTilemap;
            _obstaclesTilemap = obstaclesTilemap;
            _floraTilemap     = floraTilemap;
            _tileSets         = tileSets;
            _mapLogic         = mapTiles;

            // Example adjacency dictionary (you can adapt):
            // "Surface" corresponds to "Grass", "Ground" to "Dirt".
            // The indices (like [0], [1], [6], etc.) refer to whichever tile arrangement you have.
            _neighbourTupleToTile = new Dictionary<Tuple<TileState, TileState, TileState, TileState>, int>
            {
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Surface, TileState.Surface),
                    6
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Ground, TileState.Surface),
                    13
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Surface, TileState.Ground),
                    0
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Ground, TileState.Ground),
                    8
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Ground, TileState.Ground),
                    15
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Ground, TileState.Surface),
                    1
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Surface, TileState.Ground),
                   11
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Surface, TileState.Surface),
                    3
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Ground, TileState.Ground),
                    9
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Surface, TileState.Surface),
                    5
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Surface, TileState.Surface),
                    2
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Ground, TileState.Surface),
                    10
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Surface, TileState.Surface, TileState.Ground),
                    7
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Surface, TileState.Surface, TileState.Ground),
                    14
                },
                {
                    Tuple.Create(TileState.Surface, TileState.Ground, TileState.Ground, TileState.Surface),
                   4
                },
                {
                    Tuple.Create(TileState.Ground, TileState.Ground, TileState.Ground, TileState.Ground),
                    12
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
                    var finalTile = tileSet[tileData.TileType][_neighbourTupleToTile[CreateTileStateTuple(cellPos)]];
                    
                    if (mapTiles.WalkableArea[x, y])
                    {
                        _baseTilemap.SetTile(cellPos, finalTile);
                    }
                    else
                    {
                        _obstaclesTilemap.SetTile(cellPos, finalTile);
                    }

                    // ------ Debug Lines & Text ------
                    if (debug)
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
        private TileState GetDisplayTileState(Vector3Int pos)
        {
            return _mapLogic.MapTiles[pos.x, pos.y].State;
        }

        private Tuple<TileState, TileState, TileState, TileState> CreateTileStateTuple(Vector3Int coords)
        {
            // For example, we define offsets:
            // topLeft => coords + (0,1)
            // topRight => coords + (1,1)
            // botLeft => coords
            // botRight => coords + (1,0)
            
            TileState topLeft  = GetDisplayTileState(coords + new Vector3Int(0, 1, 0));
            TileState topRight = GetDisplayTileState(coords + new Vector3Int(1, 1, 0));
            TileState botLeft  = GetDisplayTileState(coords);
            TileState botRight = GetDisplayTileState(coords + new Vector3Int(1, 0, 0));

            return Tuple.Create(topLeft, topRight, botLeft, botRight);
        }
    }

    /// <summary>
    /// 用于区分相邻格子的类型(单纯演示)。Grass -> Surface, Dirt -> Ground
    /// </summary>

}