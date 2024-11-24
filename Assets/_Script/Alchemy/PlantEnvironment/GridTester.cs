using System;
using _Script.Alchemy.PlantEnvironment;
using _Script.Utilities;
using UnityEditor;
using UnityEngine;
namespace _Script.Alchemy.Pot
{
    [ExecuteInEditMode]
    public class GridTester : MonoBehaviour
    {
        [SerializeField] private OTileMap tileMap;
        [SerializeField] private GridRenderer gridRenderer;
        
        [SerializeField] private bool _needsUpdate = false;
        [SerializeField] private bool _canEdit = false;
        private void Awake()
        {
  
        }
        
        private void Start()
        {
            tileMap = GetComponent<OTileMap>();
            tileMap.Initialize(10, 5, 1f, transform.position);
            gridRenderer = GetComponent<GridRenderer>();
            gridRenderer.SetGrid(tileMap.Grid);
        }
        
        
        
        private void Update()
        {
            if(_needsUpdate && _canEdit)
            {
                if (tileMap.Grid == null)
                {
                    tileMap = GetComponent<OTileMap>();
                    tileMap.Initialize(10, 5, 1f, transform.position);
                    gridRenderer = GetComponent<GridRenderer>();
                    gridRenderer.SetGrid(tileMap.Grid);
                    _needsUpdate = false;
                    gridRenderer.InitializeMaterials();
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
            //     tileMap.SetTileType(position, TileType.Dirt);
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
                tileMap.SetTileType(position, TileType.Dirt);
                _needsUpdate = true;
                
                e.Use(); // mark the event as "used" so it doesn't propagate
            }
        }
        
        private class GridMapVisual
        {
            private Grid _grid;
            
        } 
    }
}