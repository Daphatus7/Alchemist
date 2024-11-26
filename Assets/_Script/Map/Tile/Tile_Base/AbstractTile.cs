using System;
using System.Collections.Generic;

namespace _Script.Map.Tile.Tile_Base
{
    /**
     * Decorator pattern
     * 
     */
    public abstract class AbstractTile
    {
        /**
         * Enforce to assign a tile type at the concrete class
         */
        protected abstract TileType TileType { get; }
        
        /**
         * Decorator pattern
         * Will apply to the surface tile
         */
        public abstract void Use();

        public abstract TileType GetTileType();

        public abstract List<TileType> GetTileTypes();
        
        public abstract List<TileSaveObject> OnSaveData();
        
        public abstract AbstractTile GetParentTile();
    }
    
    [Serializable]
    public abstract class TileSaveObject
    {
        public TileType TileType;
    }
    
    public enum TileType
    {
        None,
        Grass,
        Path,
        Rock,
        Dirt,
        WetDirt 
    }
    
    public enum TileCategory
    {
        None,
        Surface,
        Plant,
        Building
    }
}