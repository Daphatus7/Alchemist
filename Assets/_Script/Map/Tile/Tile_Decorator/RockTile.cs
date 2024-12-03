using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Decorator
{
    
    [CreateAssetMenu(fileName = "T_Rock", menuName = "Tile/T_Rock")]
    public class RockTile : TileDecorator
    {
        
        protected override TileType TileType => TileType.Rock;

        public RockTile(CustomTile baseTile) : base(baseTile)
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