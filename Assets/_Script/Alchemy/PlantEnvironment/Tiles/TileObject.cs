using UnityEngine;

namespace _Script.Alchemy.PlantEnvironment
{
    public class TileObject 
    {
        // parent grid
        private readonly int _x; public int X => _x;
        private readonly int _y; public int Y => _y;
        private TileType _tileType; public TileType TileType => _tileType;
        private IGridTileHandle _gridTileHandle;
        
        public TileObject(int x, int y, IGridTileHandle gridTileHandle,TileType tileType = TileType.Grass)
        {
            _x = x;
            _y = y;
            _tileType = tileType;
            _gridTileHandle = gridTileHandle;
        }

        public void SetTileType(TileType tileType) 
        {
            _tileType = tileType;
            _gridTileHandle.OnUpdate(_x, _y);
        }

        public override string ToString() 
        {
            return _tileType.ToString();
        }
    }
    
    public enum TileType 
    {
        None,
        Grass,
        Path,
        Dirt,
        WetDirt 
    }
}