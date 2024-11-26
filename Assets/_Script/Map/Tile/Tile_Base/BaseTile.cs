using System;
using System.Collections.Generic;
using _Script.Map.GridMap;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Base
{
    public abstract class BaseTile : AbstractTile
    {
        protected int _x; public int X => _x;
        protected int _y; public int Y => _y;
        protected bool _isWet; public bool IsWet => _isWet;
        protected IGridTileHandle GridTileHandle;
        
        public BaseTile(int x, int y, bool isWet, IGridTileHandle gridTileHandle)
        {
            _x = x;
            _y = y;
            _isWet = isWet;
            GridTileHandle = gridTileHandle;
        }
        
        public override List<TileSaveObject> OnSaveData()
        {
            var baseTileSaveObject = new TileBaseSaveObject(TileType, _x, _y, _isWet);
            return new List<TileSaveObject> {baseTileSaveObject};
        }
    }
    
    [Serializable]
    public class TileBaseSaveObject : TileSaveObject
    {
        public int X;
        public int Y;
        public bool IsWet;
        public TileBaseSaveObject(TileType tileType, int x, int y, bool isWet)
        {
            TileType = tileType;
            X = x;
            Y = y;
            IsWet = isWet;
        }
    }
}