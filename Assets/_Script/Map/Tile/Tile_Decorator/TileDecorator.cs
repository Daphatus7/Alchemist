using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Decorator
{
    public abstract class TileDecorator : AbstractTile
    {
        protected readonly AbstractTile ParentTile;
        
        protected TileDecorator(AbstractTile parentTile)
        {
            ParentTile = parentTile;
        }
        
        public override AbstractTile GetParentTile()
        {
            return ParentTile;
        }
        public override TileType GetTileType()
        {
            return TileType;
        }

        public override List<TileType> GetTileTypes()
        {
            var tileTypes = ParentTile.GetTileTypes();
            tileTypes.Add(TileType);
            return tileTypes;
        }

        public override List<TileSaveObject> OnSaveData()
        {
            var tileSaveObjects = ParentTile.OnSaveData();
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