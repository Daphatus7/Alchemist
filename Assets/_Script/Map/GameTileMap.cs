using System;
using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.Tile.Tile_Base;
using _Script.Utilities;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map
{

    [DefaultExecutionOrder(10)]
    public class GameTileMap : Singleton<GameTileMap>, ISaveTileMap
    {
        private Tilemap tileMap;
        
        private static TileContext _pointedTile; public static TileContext PointedTile => _pointedTile;
        
        private readonly Dictionary<Vector2Int, Crop> _crops = new Dictionary<Vector2Int, Crop>();
        
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
                //try to access the dictionary
                if(_pointedTile != null)
                {
                    //check if the key exist
                    if(_crops.ContainsKey(_pointedTile.Position))
                    {
                        //get the crop
                        var crop = _crops[_pointedTile.Position];
                        //harvest the crop
                        if(crop.Harvest())
                        {
                            //remove the crop from the dictionary
                            Destroy(_crops[_pointedTile.Position].gameObject);
                            _crops.Remove(_pointedTile.Position);
                        }
                    }
                }
            }
            
        }

        #region Crop

        private bool IsFertile(CustomTile tile, Vector2Int position)
        {
            if(tile.GetTileType() == TileType.Soil)
            {
                return !_crops.ContainsKey(position);
            }
            return false;
        }
        
        
        
        public bool AddCrop(GameObject cropPrefab)
        {
            if(_pointedTile != null)
            {
                if (_pointedTile.GetTile.GetTileType() == TileType.Soil)
                {
                    //check if the key exist - if the key exist, then there is a crop
                    if (_crops.ContainsKey(_pointedTile.Position))
                    {
                        return false;
                    }
                    else
                    {
                        var crop = Instantiate(cropPrefab, _pointedTile.WorldPosition, Quaternion.identity).GetComponent<Crop>();
                        _crops.Add(_pointedTile.Position, crop);
                        return true;
                    }
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
                _pointedTile = new TileContext(tileMap.GetTile<CustomTile>(cellPosition), 
                    new Vector2Int(cellPosition.x, cellPosition.y), 
                    worldPosition, IsFertile(tile, currentCursorCellPosition));
                OnCursorMoved?.Invoke(currentCursorPosition);
            }
        }
        

        #endregion

    }
}