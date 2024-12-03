using System;
using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.GridMap;
using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Concrete
{
    [CreateAssetMenu(fileName = "T_Soil", menuName = "Tile/T_Soil")]
    public class SoilTile : BaseTile
    {
        protected override TileType TileType => TileType.Soil;

        public bool IsFertile
        {
            get
            {
                return true;
            }
        }
        
        public SoilTile(int x, int y,IGridTileHandle gridTileHandle) : base(x, y, false, gridTileHandle)
        {
        }
        
        public override TileType GetTileType()
        {
            return IsWet ? TileType.WetSoil : TileType.Soil;
        }

        public override List<TileType> GetTileTypes()
        {
            return IsWet ? new List<TileType> {TileType, TileType.WetSoil} : new List<TileType> {TileType};
        }

        public override CustomTile GetParentTile()
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
            // if(_crop)
            // {
            //     if(_crop.Harvest())
            //     {
            //         Destroy(_crop.gameObject);
            //         _crop = null;
            //     }
            // }
            
        }

        public void AddCrop(Crop crop)
        {
            //_crop = crop;
        }
    }
}