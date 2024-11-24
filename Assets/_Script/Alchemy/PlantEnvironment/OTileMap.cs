using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.Alchemy.PlantEnvironment
{
    // Creator Class for grids
    public class OTileMap : MonoBehaviour
    {
        private Grid<TileObject> _grid; public Grid<TileObject> Grid => _grid;
        
        public void Initialize(int width, int height, float cellSize, Vector3 originPosition)
        {
            _grid = new Grid<TileObject>(width, height, cellSize, originPosition, (int x, int y, Grid<TileObject> g) => new TileObject(x, y, g));
        }

        public void SetTileType(Vector3 worldPosition, TileType tileType)
        {
            var tilemapObject = _grid.GetGridObject(worldPosition);
            if (tilemapObject != null) 
            {
                tilemapObject.SetTileType(tileType);
            }
            else
            {
                Debug.LogWarning("TilemapObject is null");
            }
        }
        
        public TileObject CreateTile(TileType tileType)
        {
            switch (tileType)
            {
                case TileType.Dirt:
                    return new DirtTile(0, 0, _grid);
                default:
                    return new TileObject(0, 0, _grid);
            }
        }
    }
}