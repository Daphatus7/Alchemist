using System.Collections.Generic;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.Tile.Tile_Concrete;
using _Script.Map.Tile.Tile_Decorator;
using UnityEngine;

namespace _Script.Map.Tile
{
    public class TileTester : MonoBehaviour
    {
        
        private void Start()
        {
            List<string> tileTypes = new List<string> { "Dirt", "Rock", "Grass" };
            AbstractTile newTile = TileFactory.CreateTile(tileTypes, 0, 0, false, null);
            var save = newTile.OnSaveData();
            // foreach (var tileType in newTile.GetTileTypes())
            // {
            //     Debug.Log(tileType);
            // }
            foreach (var tileSaveObject in save)
            {
                Debug.Log(tileSaveObject.TileType);
            }
        }
    }
}