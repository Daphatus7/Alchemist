using System.Collections.Generic;
using _Script.Map.Tile;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.Tile.Tile_Concrete;
using UnityEngine;

namespace _Script.Map.GridMap
{
    // Creator Class for grids
    public class OTileMap : MonoBehaviour
    {
        private Grid<AbstractTile> _grid; public Grid<AbstractTile> Grid => _grid;
        
        /**
         * Initialize the grid with the 
         */
        public void Initialize(int width, int height, float cellSize, Vector3 originPosition)
        {
            _grid = new Grid<AbstractTile>(width, height, cellSize, originPosition, (int x, int y, Grid<AbstractTile> g) => new DirtTile(x, y, g));
        }
        
        public void LoadSavedData(TileMapSave tileMapSave)
        {
            
            var tilesData = tileMapSave.TileSaveObjects;
            var width = tileMapSave.Width;
            var height = tileMapSave.Height;
            var cellSize = tileMapSave.CellSize;
            var originPosition = tileMapSave.OriginPosition;
            
            _grid = new Grid<AbstractTile>(width, height, cellSize, originPosition, (int x, int y, Grid<AbstractTile> g) =>
            {
                var index = x * height + y;
                var tileData = tilesData[x * height + y];
                if (tilesData[x * height + y] == null)
                {
                    return new DirtTile(x, y, g);
                }
                else
                {
                    return TileFactory.LoadSavedTile(tileData, x, y, g);
                }
            });
        }
        
        public object OnSaveData()
        {
            
            List<List<TileSaveObject>> tileSaveObjects = new List<List<TileSaveObject>>();
            
            // Save all the tiles
            for (int x = 0; x < _grid.GetWidth(); x++)
            {
                for (int y = 0; y < _grid.GetHeight(); y++)
                {
                    var tile = _grid.GetGridArray()[x, y];
                    if (tile != null)
                    {
                        tileSaveObjects.Add(tile.OnSaveData());
                    }
        
                }
            }
            
            //pack as a TileMapSave object
            TileMapSave tileMapSave = new TileMapSave(
                tileSaveObjects,
                _grid.GetWidth(),
                _grid.GetHeight(),
                _grid.GetCellSize(),
                _grid.GetOriginPosition()
            );

            return tileMapSave;
        }
       
        
        public void SetTile(Vector3 pos, List<TileType> tileTypes)
        {
            int x, y;
            _grid.GetXY(pos, out x, out y);
            _grid.GetGridArray()[x, y] = TileFactory.CreateTile(tileTypes, x, y, _grid);
            //trigger update
            _grid.OnUpdate(x, y);
        }
        
    }
}