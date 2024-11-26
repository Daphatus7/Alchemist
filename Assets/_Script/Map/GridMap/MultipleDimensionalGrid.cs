using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;

namespace _Script.Map.GridMap
{
    
    /*
     * The grid contains multiple layers
     * and built upon the base grid.
     * When interact with the base grid, the interaction will effectively send to the layer above it
     * This communication is achieved using OnValueChanged event
     */
    public class MultipleDimensionalGrid
    {
        private Grid<BaseTile> _baseGrid; // Ground
        private Grid<BaseTile> _vegetationGrid; // Vegetation
    }
}