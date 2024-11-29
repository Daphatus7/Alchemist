using System.Collections.Generic;
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
    //[ExecuteInEditMode]
    public class GameTileMap : Singleton<GameTileMap>, ISaveTileMap
    {
        [SerializeField] private OTileMap tileMap;
        [SerializeField] private TileGridRenderer gridRenderer;
        
        [SerializeField] private bool _needsUpdate = false;
        [SerializeField] private bool _canEdit = false;
        
        
        private TileType _pointedTileType; public TileType PointedTileType => _pointedTileType;
        
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
            UpdateCursorTileType(Helper.GetMouseWorldPosition());
            
            
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
            //     Debug.Log("Mouse Clicked");
            //     Vector3 position = Helper.GetMouseWorldPosition();
            //     tileMap.SetTile(position, new List<TileType> {TileType.Dirt, TileType.Grass});
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
        
        
        public void UpdateCursorTileType(Vector3 position)
        {
            if(CursorMovementTracker.Instance.HasCursorMoved)
            {
                _pointedTileType = tileMap.GetTileType(position);
                Debug.Log(_pointedTileType);
            }
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
        
    }
}