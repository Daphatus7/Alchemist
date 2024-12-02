using System;
using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile
{
    [Serializable]
    public class TileMapSave
    {
        /**
         * 2D array of TileSaveObject
         * - Tile - base
         * - Decorators for the tile
         */
        public List<List<TileSaveObject>> TileSaveObjects;
        public int Width;
        public int Height;
        public float CellSize;
        public Vector3 OriginPosition;

        public TileMapSave(List<List<TileSaveObject>> tileSaveObjects, int width, int height, float cellSize, Vector3 originPosition)
        {
            TileSaveObjects = tileSaveObjects;
            Width = width;
            Height = height;
            CellSize = cellSize;
            OriginPosition = originPosition;
        }
    }
}