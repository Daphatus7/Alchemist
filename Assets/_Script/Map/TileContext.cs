using _Script.Alchemy.Plant;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.Tile.Tile_Concrete;
using UnityEngine;

namespace _Script.Map
{
    /**
     * Stores the information of currently selected tile
     * Shows all possible interaction of the tile
     */
    public class TileContext
    {
        
        //The surface tile
        private readonly AbstractTile _tile;
        public TileContext(AbstractTile tile)
        {
            _tile = tile;
        }
        
        public TileType TileType => _tile.GetTileType();
        

        public Vector2Int Position
        {
            get
            {
                return new Vector2Int(_tile.GetBaseTile().X,
                    _tile.GetBaseTile().Y);
            }
        }
        
        public void Use()
        {
            if (_tile != null)
            {
                _tile.Use();
            }
            else
            {
                Debug.Log("Tile is null");
            }
        }
        
        public bool AddCrop(Crop crop)
        {
            if (_tile.GetBaseTile() is SoilTile soilTile)
            {
                if(soilTile.IsFertile)
                {
                    soilTile.AddCrop(crop);
                    return true;
                }
            }
            return false;
        }
    }
}