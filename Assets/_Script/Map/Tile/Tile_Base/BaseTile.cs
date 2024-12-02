using System;
using System.Collections.Generic;
using _Script.Map.GridMap;
using UnityEngine;

namespace _Script.Map.Tile.Tile_Base
{
    public abstract class BaseTile : AbstractTile
    {
        private readonly int _x; public int X => _x;
        private readonly int _y; public int Y => _y;
        private readonly bool _isWet; public bool IsWet => _isWet;
        private IGridTileHandle GridTileHandle { get; }

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
        
        public void OnUpdate()
        {
            GridTileHandle.OnUpdate(_x, _y);
        }
    }
    
    [Serializable]
    public class TileBaseSaveObject : TileSaveObject
    {
        public bool IsWet;
        public TileBaseSaveObject(TileType tileType, int x, int y, bool isWet)
        {
            TileType = tileType;
            IsWet = isWet;
        }
    }
}