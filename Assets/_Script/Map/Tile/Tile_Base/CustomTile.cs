using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Tile.Tile_Base
{
    /**
     * Decorator pattern
     *
     */
    public abstract class CustomTile : UnityEngine.Tilemaps.Tile
    {
        /**
     * Enforce to assign a tile type at the concrete class
     */
        protected abstract TileType TileType { get; }
        
        [SerializeField] private String tileName;
        
        public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
        {
            tileData.sprite = sprite;
        }
        /**
     * Decorator pattern
     * Will apply to the surface tile
     */
        public abstract void Use();

        public abstract TileType GetTileType();

        public abstract List<TileType> GetTileTypes();
        
        public abstract List<TileSaveObject> OnSaveData();
        
        public abstract CustomTile GetParentTile();
        
        public abstract BaseTile GetBaseTile();
        
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
        Soil,
        Rock,
        Dirt,
        WetDirt,
        WetSoil
    }
    
    public enum TileCategory
    {
        None,
        Surface,
        Plant,
        Building
    }
}