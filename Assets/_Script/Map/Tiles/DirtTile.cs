using _Script.Alchemy.Plant;
using _Script.Alchemy.PlantEnvironment;
using _Script.Map.GridMap;

namespace _Script.Map.Tiles
{
    public class DirtTile : TileObject
    {
        private bool _isWet;
        private bool _isFertile;
        private Crop _crop;
        
        public DirtTile(int x, int y, IGridTileHandle gridTileHandle, TileType tileType = TileType.Grass) : base(x, y, gridTileHandle, tileType)
        {
        }
        
        public void Water()
        {
            _isWet = true;
            SetTileType(TileType.WetDirt);
        }
    }
}