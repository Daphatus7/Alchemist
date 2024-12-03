using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Decorator
{
    public abstract class TileDecorator : CustomTile
    {
        private readonly CustomTile _parentTile;
        
        protected TileDecorator(CustomTile parentTile)
        {
            _parentTile = parentTile;
        }

        protected TileDecorator()
        {
        }

        public override BaseTile GetBaseTile()
        {
            return _parentTile.GetBaseTile();
        }
        
        public override CustomTile GetParentTile()
        {
            return _parentTile;
        }
        public override TileType GetTileType()
        {
            return TileType;
        }

        public override List<TileType> GetTileTypes()
        {
            var tileTypes = _parentTile.GetTileTypes();
            tileTypes.Add(TileType);
            return tileTypes;
        }

        public override List<TileSaveObject> OnSaveData()
        {
            var tileSaveObjects = _parentTile.OnSaveData();
            tileSaveObjects.Add(new TileDecoratorSaveObject(TileType));
            return tileSaveObjects;
        }
    }
    
    public class TileDecoratorSaveObject : TileSaveObject
    {
        public TileDecoratorSaveObject(TileType type)
        {
            TileType = type;
        }
    }
}