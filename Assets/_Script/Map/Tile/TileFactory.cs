using System.Collections.Generic;
using System.Linq;
using _Script.Map.GridMap;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.Tile.Tile_Concrete;
using _Script.Map.Tile.Tile_Decorator;

namespace _Script.Map.Tile
{
    /**
     * Factory pattern
     * To load a tile from the saved data
     * A tile can be wrapped in multiple decorators
     */
    public class TileFactory
    {
        public static Dictionary<string, TileType> TileTypeMap = new Dictionary<string, TileType>
        {
            {"DirtTile", TileType.Dirt},
            {"GrassTile", TileType.Grass}
        };
        
        public static AbstractTile CreateTile(List<string> type, int x, int y, object data, IGridTileHandle gridTileHandle)
        {
            //Base tile
            AbstractTile baseTile = null;
            switch (type[0])
            {
                case "Dirt":
                    baseTile = LoadDirtTile(x, y, (bool) data, gridTileHandle);
                    break;
                Default:
                    //Exception
                    throw new System.Exception("The first tile of the stack is not a valid tile type");
                    return null;
            }
            
            //Decorators
            for (int i = 1; i < type.Count; i++)
            {
                switch (type[i])
                {
                    case "Grass":
                        baseTile = new GrassTile(baseTile);
                        break;
                    case "Rock":
                        baseTile = new RockTile(baseTile);
                        break;
                }
            }
            
            return baseTile;
        }


        #region BaseTile
        
        private static DirtTile LoadDirtTile(int x, int y, bool isWet, IGridTileHandle gridTileHandle)
        {
            return new DirtTile(x, y, isWet, null); 
        }
        
        #endregion

        
        #region TileDecorator
        //decorator classes
        
        #endregion
    }
}