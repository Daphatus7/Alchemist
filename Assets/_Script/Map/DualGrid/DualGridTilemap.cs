// Author : Peiyu Wang @ Daphatus
// 22 12 2024 12 50

using System;
using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.DualGrid
{
    public class DualGridTilemap : MonoBehaviour
    {
        protected static Vector3Int[] NEIGHBOURS = new Vector3Int[]
        {
            new Vector3Int(0, 0, 0),
            new Vector3Int(1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(1, 1, 0)
        };

        protected static Dictionary<Tuple<TileType, TileType, TileType, TileType>, UnityEngine.Tilemaps.Tile>
            neighbourTupleToTile;

        // Provide references to each tilemap in the inspector
        public Tilemap placeholderTilemap;
        public Tilemap displayTilemap;

        // Provide the dirt and grass placeholder tiles in the inspector
        public UnityEngine.Tilemaps.Tile grassPlaceholderTile;
        public UnityEngine.Tilemaps.Tile dirtPlaceholderTile;

        // Provide the 16 tiles in the inspector
        public UnityEngine.Tilemaps.Tile[] tiles;

        
        
        void Start()
        {
            //fill placeholderTilemap with grass
            
            for (int i = -50; i < 50; i++)
            {
                for (int j = -50; j < 50; j++)
                {
                    placeholderTilemap.SetTile(new Vector3Int(i, j, 0), grassPlaceholderTile);
                }
            }
            
            // This dictionary stores the "rules", each 4-neighbour configuration corresponds to a tile
            // |_1_|_2_|
            // |_3_|_4_|
            neighbourTupleToTile = new Dictionary<Tuple<TileType, TileType, TileType, TileType>, UnityEngine.Tilemaps.Tile>
            {
                { new(TileType.Grass, TileType.Grass, TileType.Grass, TileType.Grass), tiles[6] },
                { new(TileType.Dirt, TileType.Dirt, TileType.Dirt, TileType.Grass), tiles[13] }, // OUTER_BOTTOM_RIGHT
                { new(TileType.Dirt, TileType.Dirt, TileType.Grass, TileType.Dirt), tiles[0] }, // OUTER_BOTTOM_LEFT
                { new(TileType.Dirt, TileType.Grass, TileType.Dirt, TileType.Dirt), tiles[8] }, // OUTER_TOP_RIGHT
                { new(TileType.Grass, TileType.Dirt, TileType.Dirt, TileType.Dirt), tiles[15] }, // OUTER_TOP_LEFT
                { new(TileType.Dirt, TileType.Grass, TileType.Dirt, TileType.Grass), tiles[1] }, // EDGE_RIGHT
                { new(TileType.Grass, TileType.Dirt, TileType.Grass, TileType.Dirt), tiles[11] }, // EDGE_LEFT
                { new(TileType.Dirt, TileType.Dirt, TileType.Grass, TileType.Grass), tiles[3] }, // EDGE_BOTTOM
                { new(TileType.Grass, TileType.Grass, TileType.Dirt, TileType.Dirt), tiles[9] }, // EDGE_TOP
                { new(TileType.Dirt, TileType.Grass, TileType.Grass, TileType.Grass), tiles[5] }, // INNER_BOTTOM_RIGHT
                { new(TileType.Grass, TileType.Dirt, TileType.Grass, TileType.Grass), tiles[2] }, // INNER_BOTTOM_LEFT
                { new(TileType.Grass, TileType.Grass, TileType.Dirt, TileType.Grass), tiles[10] }, // INNER_TOP_RIGHT
                { new(TileType.Grass, TileType.Grass, TileType.Grass, TileType.Dirt), tiles[7] }, // INNER_TOP_LEFT
                { new(TileType.Dirt, TileType.Grass, TileType.Grass, TileType.Dirt), tiles[14] }, // DUAL_UP_RIGHT
                { new(TileType.Grass, TileType.Dirt, TileType.Dirt, TileType.Grass), tiles[4] }, // DUAL_DOWN_RIGHT
                { new(TileType.Dirt, TileType.Dirt, TileType.Dirt, TileType.Dirt), tiles[12] },
            };
            RefreshDisplayTilemap();
        }

        public void SetCell(Vector3Int coords, UnityEngine.Tilemaps.Tile tile)
        {
            placeholderTilemap.SetTile(coords, tile);
            SetDisplayTile(coords);
        }

        private TileType GetPlaceholderTileTypeAt(Vector3Int coords)
        {
            if (placeholderTilemap.GetTile(coords) == grassPlaceholderTile)
                return TileType.Grass;
            else
                return TileType.Dirt;
        }

        protected UnityEngine.Tilemaps.Tile CalculateDisplayTile(Vector3Int coords)
        {
            // 4 neighbours
            TileType topRight = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[0]);
            TileType topLeft = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[1]);
            TileType botRight = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[2]);
            TileType botLeft = GetPlaceholderTileTypeAt(coords - NEIGHBOURS[3]);

            Tuple<TileType, TileType, TileType, TileType> neighbourTuple = new(topLeft, topRight, botLeft, botRight);

            return neighbourTupleToTile[neighbourTuple];
        }

        protected void SetDisplayTile(Vector3Int pos)
        {
            for (int i = 0; i < NEIGHBOURS.Length; i++)
            {
                Vector3Int newPos = pos + NEIGHBOURS[i];
                displayTilemap.SetTile(newPos, CalculateDisplayTile(newPos));
            }
        }

        // The tiles on the display tilemap will recalculate themselves based on the placeholder tilemap
        public void RefreshDisplayTilemap()
        {
            for (int i = -50; i < 50; i++)
            {
                for (int j = -50; j < 50; j++)
                {
                    SetDisplayTile(new Vector3Int(i, j, 0));
                }
            }
        }
    }
}