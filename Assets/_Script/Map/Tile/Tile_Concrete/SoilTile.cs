using System;
using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.GridMap;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Concrete
{
    public class SoilTile : BaseTile
    {
        protected override TileType TileType => TileType.Dirt;
        private Crop _crop;
        
        public SoilTile(int x, int y,IGridTileHandle gridTileHandle) : base(x, y, false, gridTileHandle)
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

        public override BaseTile GetBaseTile()
        {
            return this;
        }

        public override void Use()
        {
            //if the plant is already grown, harvest it
            //try to get the seed that the player is holding
            //if indeed the player is holding a seed, plant it, remove the seed from the player's inventory
        }
    }
}