using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;

namespace _Script.Map.Tile.Tile_Decorator
{
    public class RockTile : TileDecorator
    {
        
        protected override TileType TileType => TileType.Rock;

        public RockTile(AbstractTile baseTile) : base(baseTile)
        {

        }
        
        public override void Use()
        {
            
        }
        
    }
    
    public class RockTileSaveObject : TileSaveObject
    {
        public RockTileSaveObject()
        {
            TileType = TileType.Rock;
        }
    }
}