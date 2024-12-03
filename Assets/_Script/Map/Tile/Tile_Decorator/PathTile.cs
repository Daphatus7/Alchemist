// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 53

using _Script.Map.Tile.Tile_Base;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Decorator
{
    public class PathTile : TileDecorator
    {
        protected override TileType TileType => TileType.Path;
        public override void Use()
        {
            Debug.Log("PathTile");
        }
    }
    
}