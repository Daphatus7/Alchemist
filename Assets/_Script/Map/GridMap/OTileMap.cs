using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.GridMap
{
    // Creator Class for grids
    public class OTileMap : MonoBehaviour
    {
        private Grid<BaseTile> _grid; public Grid<BaseTile> Grid => _grid;
        
        public void Initialize(int width, int height, float cellSize, Vector3 originPosition)
        {
            //TODO: Implement this
            //_grid = new Grid<BaseTile>(width, height, cellSize, originPosition, (int x, int y, Grid<BaseTile> g) => new BaseTile(x, y, g));
        }
        
        public void Initialize(int width, int height, float cellSize, Vector3 originPosition, List<TileSaveObject> tileSaveObjects)
        {
            
            // _grid = new Grid<BaseTile>(width, height, cellSize, originPosition, (int x, int y, Grid<BaseTile> g) =>
            // {
            //     var tileSaveObject = tileSaveObjects.Find(tile => tile.X == x && tile.Y == y);
            //     if (tileSaveObject != null)
            //     {
            //         var tile = ServiceLocator.Instance.Get<ITileFactory>().CreateTile(tileSaveObject.ClassName, x, y);
            //         tile.SetTileType(tileSaveObject.TileType);
            //         return tile;
            //     }
            //     return new BaseTile(x, y, g);
            // });
        }
        
        public void SetTileType(Vector3 worldPosition, TileType tileType)
        {
            var tilemapObject = _grid.GetGridObject(worldPosition);
            if (tilemapObject != null) 
            {
                Debug.LogError("SetTileType to be removed from OTileMap");
                //tilemapObject.SetTileType(tileType);
            }
            else
            {
                Debug.LogWarning("TilemapObject is null");
            }
        }

       
    }
}