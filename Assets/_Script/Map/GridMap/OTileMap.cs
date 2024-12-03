using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.Tile;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.Tile.Tile_Concrete;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.U2D;

namespace _Script.Map.GridMap
{
    // Creator Class for grids
    public class OTileMap : MonoBehaviour
    {

        private Tilemap _tilemap;
        private Grid _grid;
        private Dictionary<TileType, CustomTile> _tileTypeToTile;
        [SerializeField] private SpriteAtlas tileAtlas;
        
        public void InitializeTileMap(int width, int height, float cellSize, Vector3 originPosition)
        {
            _tilemap = GetComponent<Tilemap>();
            TileType defaultTileType = TileType.Dirt;
            CustomTile defaultTile = _tileTypeToTile[defaultTileType];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3Int tilePosition = new Vector3Int(x, y, 0);
                    _tilemap.SetTile(tilePosition, defaultTile);
                }
            }
        } 
        
        public void InitializeTileTypeDictionary(Dictionary<TileType, CustomTile> tileTypeToTile)
        {
            _tileTypeToTile = tileTypeToTile;
        }
        public void LoadSavedData(TileMapSave tileMapSave)
        {
            
        }
        
        public object OnSaveData()
        {
            return null;
        }
        
        public void SetTile(Vector3 worldPosition, List<TileType> tileTypes)
        {
            Vector3Int tilePosition = _tilemap.WorldToCell(worldPosition);
            _tilemap.SetTile(tilePosition, _tileTypeToTile[tileTypes[0]]);
        }
        
        
        /**
         * Try to remove the surface of the tile
         * If there is no more surface, then show warning
         */
        public void DestroySurfaceTile(Vector3 worldPosition)
        {
            Vector3Int tilePosition = _tilemap.WorldToCell(worldPosition);
            
            var parentTile = _tilemap.GetTile<CustomTile>(tilePosition).GetParentTile();
            if (parentTile != null)
            {
                Debug.Log("Destroying surface tile");
                _tilemap.SetTile(tilePosition, parentTile);
            }
            else
            {
                Debug.Log("Cannot destroy surface tile");
            }
        }
        
        public Vector3 GetTilePosition(Vector3 worldPosition)
        {
            return _tilemap.WorldToCell(worldPosition);
        }

        public void Use(Vector3 worldPosition)
        {
            Vector3Int tilePosition = _tilemap.WorldToCell(worldPosition);
            CustomTile tile = _tilemap.GetTile<CustomTile>(tilePosition);
            
            if (tile != null)
            {
                tile.Use();
            }
            else
            {
                Debug.Log("No tile to use at this position.");
            }
        }

        public TileType GetTileType(Vector3 position)
        {
            Vector3Int tilePosition = _tilemap.WorldToCell(position);
            CustomTile tile = _tilemap.GetTile<CustomTile>(tilePosition);
            return tile.GetTileType();
        }

        public bool AddCrop(Vector2Int position, GameObject cropPrefab)
        {
  
            //check if the tile is soil
            CustomTile tile = _tilemap.GetTile<CustomTile>(new Vector3Int(position.x, position.y, 0));
            if(tile is SoilTile soilTile)
            {
                if (soilTile.IsFertile)
                {
                    var crop = Instantiate(cropPrefab, tile.transform.GetPosition(), Quaternion.identity).GetComponent<Crop>();
                    soilTile.AddCrop(crop);
                    return true;
                }
            }
            return false;
        }

    }
}