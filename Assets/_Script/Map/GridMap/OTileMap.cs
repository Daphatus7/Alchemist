using System.Collections.Generic;
using _Script.Alchemy.Plant;
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
            //check boundary
            if (x < 0 || y < 0 || x >= _grid.GetWidth() || y >= _grid.GetHeight())
            {
                return;
            }
            
            _grid.GetGridArray()[x, y] = TileFactory.CreateTile(tileTypes, x, y, _grid);
            //trigger update
            _grid.OnUpdate(x, y);
        }
        
        
        /**
         * Try to remove the surface of the tile
         * If there is no more surface, then show warning
         */
        public void DestroySurfaceTile(Vector3 pos)
        {
            int x, y;
            _grid.GetXY(pos, out x, out y);
            if (x < 0 || y < 0 || x >= _grid.GetWidth() || y >= _grid.GetHeight())
            {
                return;
            }
            var parentTile = _grid.GetGridArray()[x, y].GetParentTile();
            if (parentTile != null)
            {
                Debug.Log("Destroying surface tile");
                _grid.GetGridArray()[x, y] = parentTile;
            }
            else
            {
                Debug.Log("Cannot destroy surface tile");
            }
            //trigger update
            _grid.OnUpdate(x, y);
        }

        public void RemoveTile(int x, int y)
        {
            var parentTile = _grid.GetGridArray()[x, y].GetParentTile();
            if (parentTile != null)
            {
                Debug.Log("Destroying surface tile");
                _grid.GetGridArray()[x, y] = parentTile;
            }
            else
            {
                Debug.Log("Cannot destroy surface tile");
            }
            //trigger update
            _grid.OnUpdate(x, y);
        }
        
        public Vector3 GetTilePosition(Vector3 position)
        {
            int x, y;
            _grid.GetXY(position, out x, out y);
            return _grid.GetWorldPosition(x, y);
        }

        public void Use(Vector3 position)
        {
            int x, y;
            _grid.GetXY(position, out x, out y);
            if (x < 0 || y < 0 || x >= _grid.GetWidth() || y >= _grid.GetHeight())
            {
                return;
            }
            _grid.GetGridArray()[x, y].Use();
        }

        public TileType GetTileType(Vector3 position)
        {
            int x, y;
            _grid.GetXY(position, out x, out y);
            if (x < 0 || y < 0 || x >= _grid.GetWidth() || y >= _grid.GetHeight())
            {
                return TileType.None;
            }
            return _grid.GetGridArray()[x, y].GetTileType();
        }

        public bool AddCrop(Vector2Int position, GameObject cropPrefab)
        {
            //check boundary
            if (position.x < 0 || position.y < 0 || position.x >= _grid.GetWidth() || position.y >= _grid.GetHeight())
            {
                return false;
            }
            //check if the tile is soil
            var tile = _grid.GetGridArray()[position.x, position.y].GetBaseTile();
            if(tile is SoilTile soilTile)
            {
                if (soilTile.IsFertile)
                {
                    var crop = Instantiate(cropPrefab, _grid.GetGridCenterWorldPosition(position.x, position.y), Quaternion.identity).GetComponent<Crop>();
                    soilTile.AddCrop(crop);
                    return true;
                }
            }
            return false;
        }

    }
}