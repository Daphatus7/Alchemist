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
        
        /**
         * Unpack tile data and create a tile based on the data
         */
        public static AbstractTile LoadSavedTile(List<TileSaveObject> tileData, int x, int y, IGridTileHandle gridTileHandle)
        {
            //Base tile
            AbstractTile baseTile = null;
            var baseTileData = (TileBaseSaveObject) tileData[0];
            switch (baseTileData.TileType)
            {
                case TileType.Dirt:
                    baseTile = LoadDirtTile(x, y, baseTileData.IsWet, gridTileHandle);
                    break;
                case TileType.Soil:
                    baseTile = LoadSoilTile(x, y, baseTileData.IsWet, gridTileHandle);
                    break;
                Default:
                    //Exception
                    throw new System.Exception("The first tile of the stack is not a valid tile type");
                    return null;
            }
            
            //Decorators
            for (int i = 1; i < tileData.Count; i++)
            {
                switch (tileData[i].TileType)
                {
                    case TileType.Grass:
                        baseTile = new GrassTile(baseTile);
                        break;
                    case TileType.Rock:
                        baseTile = new RockTile(baseTile);
                        break;
                }
            }
            
            return baseTile;
        }

        public static AbstractTile CreateTile(List<TileType> tileTypes, int x, int y, IGridTileHandle gridTileHandle)
        {
            //Base tile
            AbstractTile baseTile = null;
            var baseTileData = tileTypes[0];
            switch (baseTileData)
            {
                case TileType.Dirt:
                    baseTile = LoadDirtTile(x, y, false, gridTileHandle);
                    break;
                case TileType.Soil:
                    baseTile = LoadSoilTile(x, y, true, gridTileHandle);
                    break;
                Default:
                    //Exception
                    throw new System.Exception("The first tile of the stack is not a valid tile type");
                    return null;
            }
            
            //Decorators
            for (int i = 1; i < tileTypes.Count; i++)
            {
                switch (tileTypes[i])
                {
                    case TileType.Grass:
                        baseTile = new GrassTile(baseTile);
                        break;
                    case TileType.Rock:
                        baseTile = new RockTile(baseTile);
                        break;
                }
            }
            
            return baseTile;
        }

        
        

        #region BaseTile
        
        private static DirtTile LoadDirtTile(int x, int y, bool isWet, IGridTileHandle gridTileHandle)
        {
            return new DirtTile(x, y, gridTileHandle);
        }
        
        private static SoilTile LoadSoilTile(int x, int y, bool isWet, IGridTileHandle gridTileHandle)
        {
            return new SoilTile(x, y, gridTileHandle);
        }
        
        #endregion

        
        #region TileDecorator
        //decorator classes
        
        #endregion
    }
}