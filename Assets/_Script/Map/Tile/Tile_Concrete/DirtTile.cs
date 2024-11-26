using System;
using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.GridMap;
using _Script.Map.Tile.Tile_Base;

namespace _Script.Map.Tile.Tile_Concrete
{
    public class DirtTile : BaseTile
    {
        protected override TileType TileType => TileType.Dirt;
        
        public DirtTile(int x, int y, IGridTileHandle gridTileHandle, bool isWet = false) : base(x, y, isWet, gridTileHandle)
        {
            
        }
        
        public override TileType GetTileType()
        {
            return IsWet ? TileType.WetDirt : TileType.Dirt;
        }

        public override List<TileType> GetTileTypes()
        {
            return IsWet ? new List<TileType> {TileType, TileType.WetDirt} : new List<TileType> {TileType};
        }

        public override AbstractTile GetParentTile()
        {
            return null;
        }

        public override void Use()
        {
            throw new System.NotImplementedException();
        }
    }
}