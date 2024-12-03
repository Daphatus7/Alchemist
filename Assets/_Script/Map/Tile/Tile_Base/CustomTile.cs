using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Tile.Tile_Base
{
    /**
     * Decorator pattern
     *
     */
    [CreateAssetMenu(fileName = "New Custom Tile", menuName = "Tiles/Custom Tile")]
    public class CustomTile : UnityEngine.Tilemaps.Tile
    {
        /**
         * Enforce to assign a tile type at the concrete class
         */

        [SerializeField] private TileType tileType; public TileType GetTileType() => tileType;
    }
    
    public enum TileType
    {
        None,
        Grass,
        Path,
        Soil,
        Rock,
        Dirt,
        WetDirt,
        WetSoil
    }
}