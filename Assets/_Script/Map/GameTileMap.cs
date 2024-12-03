using System;
using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.Tile.Tile_Concrete;
using _Script.Utilities;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map
{

    [DefaultExecutionOrder(10)]
    public class GameTileMap : Singleton<GameTileMap>, ISaveTileMap
    {
        [SerializeField] private Tilemap tileMap;
        
        private TileContext _pointedTile; public TileContext PointedTile => _pointedTile;
        
        private void Start()
        {
            tileMap = GetComponent<Tilemap>();
        }

        private void OnDestroy()
        {
        }
        
        private void Update()
        {
            
            TrackHasCursorMoved();
            
            
            if(Input.GetMouseButtonDown(0))
            {
                _pointedTile?.Use();
            }
            
        }

        #region Crop

        public bool AddCrop(GameObject cropPrefab)
        {
            if(_pointedTile == null)
            {
                return false;
            }
            
            //check if the tile is soil
            var tile = _pointedTile.GetTile;
            if(tile is SoilTile soilTile)
            {
                if (soilTile.IsFertile)
                {
                    var crop = Instantiate(cropPrefab, _pointedTile.WorldPosition, Quaternion.identity).GetComponent<Crop>();
                    soilTile.AddCrop(crop);
                    return true;
                }
            }
            return false;
        }
        
        #endregion

        public Vector3 GetTileWorldCenterPosition(int x, int y)
        {
            return tileMap.GetCellCenterWorld(new Vector3Int(x, y, 0));
        }
        

        #region Save-and-Load
        
        public object OnSaveData()
        {
            return null;
        }

        public void OnLoadData(object data)
        {

        }

        public void LoadDefaultData()
        {
        }
        
        #endregion

        #region Optimization - cursor movement tracker
        
        private Vector2Int _lastCursorPosition;
        public event Action<Vector2> OnCursorMoved;
        
        private void TrackHasCursorMoved()
        {
            // Get the current cursor position
            Vector3 currentCursorPosition = Helper.GetMouseWorldPosition();
            
            // Convert the cursor position to world position
            var cellPosition = tileMap.WorldToCell(currentCursorPosition);
            var worldPosition = tileMap.GetCellCenterWorld(cellPosition);
            // Check if the cursor has moved
            var currentCursorCellPosition = new Vector2Int(cellPosition.x, cellPosition.y);
            if(_lastCursorPosition != currentCursorCellPosition)
            {
                _lastCursorPosition = currentCursorCellPosition;
                var tile = tileMap.GetTile<CustomTile>(cellPosition);
                //if there is no tile at the cursor position
                if (tile == null)
                {
                    _pointedTile = null;
                    return;
                }
                Debug.Log("Cursor moved to " + tile.GetTileType());
                _pointedTile = new TileContext(tileMap.GetTile<CustomTile>(cellPosition), new Vector2Int(cellPosition.x, cellPosition.y), worldPosition);
                OnCursorMoved?.Invoke(currentCursorPosition);
            }
        }
        

        #endregion

    }
}