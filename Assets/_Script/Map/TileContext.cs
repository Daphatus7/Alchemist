using _Script.Map.Tile.Tile_Base;
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
        private readonly CustomTile _tile;
        private readonly Vector2 _worldPosition;
        private readonly Vector2Int _position;
        private bool _isFertile; public bool IsFertile => _isFertile;
        
        public TileContext(CustomTile tile, Vector2Int position, Vector2 worldPosition, bool isFertile)
        {
            _tile = tile;
            _position = position;
            _worldPosition = worldPosition;
            _isFertile = isFertile;
        }
        
        public TileType TileType => _tile.GetTileType();
        public CustomTile GetTile => _tile;

        public Vector2 WorldPosition => _worldPosition;
        public Vector2Int Position => _position;
        
    }
}