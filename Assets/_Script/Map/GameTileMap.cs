using System;
using System.Collections.Generic;
using _Script.Alchemy.Plant;
using _Script.Map.GridMap;
using _Script.Map.Tile;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.TileRenderer;
using _Script.Utilities;
using _Script.Utilities.ServiceLocator;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Map
{

    [DefaultExecutionOrder(10)]
    public class GameTileMap : Singleton<GameTileMap>, ISaveTileMap
    {
        [SerializeField] private OTileMap tileMap;
        [SerializeField] private TileGridRenderer gridRenderer;
        
        [SerializeField] private bool _needsUpdate = false;
        [SerializeField] private bool _canEdit = false;
        
        [SerializeField] private GameObject cropPrefab;
        
        
        private TileContext _pointedTile; public TileContext PointedTile => _pointedTile;
        
        private void Start()
        {
            ServiceLocator.Instance.Register<ISaveTileMap>(this);
        }

        private void OnDestroy()
        {
            ServiceLocator.Instance.Unregister<ISaveTileMap>(this);
        }

        private void InitializeTileMap()
        {
            tileMap = GetComponent<OTileMap>();
            tileMap.Initialize(10, 5, 1f, transform.position);
            gridRenderer = GetComponent<TileGridRenderer>();
            gridRenderer.SetGrid(tileMap.Grid);
        }
        
        private void Update()
        {
            
            TrackHasCursorMoved();
            
            if(_needsUpdate && _canEdit)
            {
                if (tileMap.Grid == null)
                {
                    tileMap = GetComponent<OTileMap>();
                    tileMap.Initialize(10, 5, 1f, transform.position);
                    gridRenderer = GetComponent<TileGridRenderer>();
                    gridRenderer.SetGrid(tileMap.Grid);
                    _needsUpdate = false;
                    //gridRenderer.InitializeMaterials();
                }
            }
            
            if (gridRenderer == null)
            {
                tileMap.Initialize(10, 5, 1f, transform.position);
                gridRenderer.SetGrid(tileMap.Grid);
            }
            // if (Input.GetMouseButtonDown(0))
            // {
            //     Debug.Log("Placing Soil");
            //     Vector3 position = Helper.GetMouseWorldPosition();
            //     tileMap.SetTile(position, new List<TileType> {TileType.Soil});
            // }
            //
            // if (Input.GetMouseButtonDown(1))
            // {
            //     Debug.Log("Mouse Clicked");
            //     Vector3 position = Helper.GetMouseWorldPosition();
            //     tileMap.Use(position);
            // }
        }
        
        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
        
        
        private void OnSceneGUI(SceneView sceneView)
        {
            if (!_canEdit) return;
            Event e = Event.current;
            // detect left mouse click
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Vector3 position = Helper.GetMouseWorldPositionInEditor();
                //tileMap.SetTileType(position, TileType.Dirt);
                _needsUpdate = true;
                
                e.Use(); // mark the event as "used" so it doesn't propagate
            }
        }

        #region Save-and-Load
        
        public object OnSaveData()
        {
            return tileMap.OnSaveData();
        }

        public void OnLoadData(object data)
        {
            tileMap = GetComponent<OTileMap>();
            
            tileMap.LoadSavedData((TileMapSave) data);
            gridRenderer = GetComponent<TileGridRenderer>();
            gridRenderer.SetGrid(tileMap.Grid);
        }

        public void LoadDefaultData()
        {
            InitializeTileMap();
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
            tileMap.Grid.GetXY(currentCursorPosition, out var x, out var y);
            
            //Out of bounds
            if(x < 0 || y < 0 || x >= tileMap.Grid.GetWidth() || y >= tileMap.Grid.GetHeight())
            {
                return;
            }
            
            // Check if the cursor has moved
            if(_lastCursorPosition != new Vector2Int(x, y))
            {
                _lastCursorPosition = new Vector2Int(x, y);
                _pointedTile = new TileContext(tileMap.Grid.GetGridArray()[x, y]);
                OnCursorMoved?.Invoke(currentCursorPosition);
            }
        }
        

        #endregion
    }
}