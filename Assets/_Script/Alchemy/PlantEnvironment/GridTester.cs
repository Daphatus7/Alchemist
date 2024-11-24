using System;
using _Script.Alchemy.PlantEnvironment;
using _Script.Utilities;
using UnityEngine;
namespace _Script.Alchemy.Pot
{
    [RequireComponent(typeof(OTileMap)), RequireComponent(typeof(OTileMapVisual))]
    public class GridTester : MonoBehaviour
    {
        private OTileMap _tileMap;
        private OTileMapVisual _tileMapVisual;
        
        private void Awake()
        {
            _tileMap = GetComponent<OTileMap>();
            _tileMapVisual = GetComponent<OTileMapVisual>();
            _tileMap.Initialize(10, 10, 1f, transform.position);
        }
        
        private void Start()
        {
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                Vector3 position = Helper.GetMouseWorldPosition();
                _tileMap.SetTileType(position, TileType.Dirt);
            }
        }
        
        private class GridMapVisual
        {
            private Grid _grid;
            
        } 
    }
}