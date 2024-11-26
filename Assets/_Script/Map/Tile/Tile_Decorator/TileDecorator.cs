using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Decorator
{
    public abstract class TileDecorator : AbstractTile
    {
        protected readonly AbstractTile BaseTile;
        
        protected TileDecorator(AbstractTile baseTile)
        {
            BaseTile = baseTile;
        }

        public override TileType GetTileType()
        {
            return TileType;
        }

        public override List<TileType> GetTileTypes()
        {
            var tileTypes = BaseTile.GetTileTypes();
            tileTypes.Add(TileType);
            return tileTypes;
        }

        public override List<TileSaveObject> OnSaveData()
        {
            var tileSaveObjects = BaseTile.OnSaveData();
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