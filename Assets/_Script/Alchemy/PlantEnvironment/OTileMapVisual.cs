using _Script.Utilities;
using UnityEngine;
using System.Collections.Generic;

namespace _Script.Alchemy.PlantEnvironment
{
    public class OTileMapVisual : MonoBehaviour
    {
        [SerializeField] private Sprite groundSprite;
        [SerializeField] private Sprite pathSprite;
        [SerializeField] private Sprite dirtSprite;
        [SerializeField] private Sprite wetDirtSprite;

        private Grid<TileObject> _grid;
        private Mesh _mesh;
        private bool _updateMesh;

        private Dictionary<TileType, UVCoords> tileUVsDictionary;

        private void Awake()
        {
            _mesh = new Mesh();

            // initialize tileUVsDictionary
            tileUVsDictionary = new Dictionary<TileType, UVCoords>();

            if (groundSprite != null)
            {
                tileUVsDictionary[TileType.Grass] = GetSpriteUVCoords(groundSprite);
            }
            if (pathSprite != null)
            {
                tileUVsDictionary[TileType.Path] = GetSpriteUVCoords(pathSprite);
            }
            if (dirtSprite != null)
            {
                tileUVsDictionary[TileType.Dirt] = GetSpriteUVCoords(dirtSprite);
            }
            if (wetDirtSprite != null)
            {
                tileUVsDictionary[TileType.WetDirt] = GetSpriteUVCoords(wetDirtSprite);
            }
        }

        public void SetGrid(Grid<TileObject> grid)
        {
            _grid = grid;
            UpdateHeatMapVisual();
            _grid.OnGridValueChanged += Grid_OnGridValueChanged;
        }

        private void Grid_OnGridValueChanged(object sender, Grid<TileObject>.OnGridValueChangedEventArgs e)
        {
            _updateMesh = true;
        }

        private void LateUpdate()
        {
            if (_updateMesh)
            {
                _updateMesh = false;
                UpdateHeatMapVisual();
            }
        }

        private Vector3[] verticesCache;
        private Vector2[] uvCache;
        private int[] trianglesCache;

        private void EnsureMeshArrays(int gridSize)
        {
            if (verticesCache == null || verticesCache.Length != gridSize * 4)
            {
                Helper.CreateEmptyMeshArrays(gridSize * 4, out verticesCache, out uvCache, out trianglesCache);
            }
        }

        private void UpdateHeatMapVisual()
        {
            int gridSize = _grid.GetWidth() * _grid.GetHeight();
            EnsureMeshArrays(gridSize);

            int index = 0;
            for (int x = 0; x < _grid.GetWidth(); x++)
            {
                for (int y = 0; y < _grid.GetHeight(); y++)
                {
                    Vector3 quadSize = new Vector3(1, 1) * _grid.GetCellSize();

                    TileObject gridObject = _grid.GetGridObject(x, y);
                    var tileType = gridObject.TileType;

                    Vector2 gridUV00, gridUV11;
                    if (tileType == TileType.None)
                    {
                        gridUV00 = Vector2.zero;
                        gridUV11 = Vector2.zero;
                        quadSize = Vector3.zero;
                    }
                    else
                    {
                        UVCoords uvCoords = tileUVsDictionary[tileType];
                        gridUV00 = uvCoords.uv00;
                        gridUV11 = uvCoords.uv11;
                    }

                    Vector3 position = _grid.GetWorldPosition(x, y) + quadSize * 0.5f;
                    Helper.AddToMeshArrays(verticesCache, uvCache, trianglesCache, index, position, 0f, quadSize, gridUV00, gridUV11);
                    index++;
                }
            }

            _mesh.Clear();
            _mesh.vertices = verticesCache;
            _mesh.uv = uvCache;
            _mesh.triangles = trianglesCache;
            _mesh.RecalculateBounds(); // only needed if the mesh can be culled
        }

        private UVCoords GetSpriteUVCoords(Sprite sprite)
        {
            Rect rect = sprite.textureRect;
            Texture texture = sprite.texture;

            float textureWidth = texture.width;
            float textureHeight = texture.height;

            Vector2 uv00 = new Vector2(rect.xMin / textureWidth, rect.yMin / textureHeight);
            Vector2 uv11 = new Vector2(rect.xMax / textureWidth, rect.yMax / textureHeight);

            return new UVCoords { uv00 = uv00, uv11 = uv11 };
        }
    }

    public struct UVCoords
    {
        public Vector2 uv00;
        public Vector2 uv11;
    }

}
