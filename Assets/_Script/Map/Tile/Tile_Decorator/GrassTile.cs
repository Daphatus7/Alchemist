using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Decorator
{
    /**
     * Use to decorate the surface tile
     * 
     */
    [CreateAssetMenu(fileName = "T_Grass", menuName = "Tile/T_Grass")]
    public class GrassTile : TileDecorator
    {
        protected override TileType TileType => TileType.Grass;
        
        public GrassTile(CustomTile tile) : base(tile)
        {
            
        }

        /**
         * Decorator pattern
         * Will apply to the surface tile
         */
        public override void Use()
        {
        }

        /**
         * Decorator pattern
         * Will apply to the surface tile
         */
        public override TileType GetTileType()
        {
            return TileType;
        }
        
    }
    
    public class GrassTileSaveObject
    {

    }
}