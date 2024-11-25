using _Script.Alchemy.Plant;
using _Script.Map.GridMap;
using _Script.Map.Tiles;
using UnityEngine;

namespace _Script.Alchemy.PlantEnvironment
{
    public class FarmMap : MonoBehaviour
    {
        //Ground
        private Grid<TileObject> _grid; public Grid<TileObject> Grid => _grid;
        
        //Crops above ground
        private Grid<Crop> _cropGrid; public Grid<Crop> CropGrid => _cropGrid;
    }
}