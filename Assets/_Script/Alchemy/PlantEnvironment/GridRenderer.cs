using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

namespace _Script.Alchemy.PlantEnvironment
{
    [ExecuteInEditMode]
    public class GridRenderer : MonoBehaviour
    {
        [SerializeField] private Mesh tileMesh; // A quad mesh
        [SerializeField] private Material baseTileMaterial; // Material with a shader that supports instancing

        [SerializeField] private SpriteAtlas spriteAtlas; // The SpriteAtlas containing all tile sprites

        private Grid<TileObject> _grid;
        public Grid<TileObject> Grid => _grid;

        private Matrix4x4[] _matrices;
        private Vector4[] _uvOffsets;
        private readonly HashSet<int> _dirtyTiles = new HashSet<int>();
        private int _gridWidth;
        private int _gridHeight;

        private readonly Matrix4x4[] _batchMatricesArray = new Matrix4x4[1023];
        private readonly Vector4[] _batchUVOffsetsArray = new Vector4[1023];

        private bool _needsUpdate = true;

        private void Awake()
        {
            if (spriteAtlas == null)
            {
                Debug.LogError("SpriteAtlas is not assigned.");
                return;
            }

            if (baseTileMaterial == null)
            {
                Debug.LogError("Base Tile Material is not assigned.");
                return;
            }

            baseTileMaterial.mainTexture = spriteAtlas.GetSprite("T_Grass_0").texture;
        }
        
        /*
         * Initialise the grid and the matrices and uvOffsets arrays
         */
        public void SetGrid(Grid<TileObject> grid)
        {
            _grid = grid;
            _gridWidth = _grid.GetWidth();
            _gridHeight = _grid.GetHeight();

            int totalTiles = _gridWidth * _gridHeight;
            _matrices = new Matrix4x4[totalTiles];
            _uvOffsets = new Vector4[totalTiles];

            // Initialize matrices and uvOffsets
            for (int i = 0; i < totalTiles; i++)
            {
                _matrices[i] = Matrix4x4.identity;
                _uvOffsets[i] = Vector4.zero;

                // Add tiles that need to be updated, which are all tiles at the start.
                _dirtyTiles.Add(i);
            }

            // Subscribe to the OnGridValueChanged event
            _grid.OnGridValueChanged += OnGridValueChanged;
            _needsUpdate = true;
        }


        private void OnDestroy()
        {
            if (_grid != null)
            {
                _grid.OnGridValueChanged -= OnGridValueChanged;
            }
        }

        // Event handler for the OnGridValueChanged event
        private void OnGridValueChanged(object sender, Grid<TileObject>.OnGridValueChangedEventArgs e)
        {
            int index = GetIndex(e.x, e.y);
            _dirtyTiles.Add(index);
            _needsUpdate = true;
        }

        // Get the index of a tile in the matrices and uvOffsets arrays
        private int GetIndex(int x, int y)
        {
            return x * _gridHeight + y;
        }

        private void LateUpdate()
        {
            
            // If the grid has changed, update the render data
            if (_needsUpdate)
            {
                UpdateRenderData();
                // Clear the flag
                _needsUpdate = false;
            }

            RenderTiles();
        }

        private void UpdateRenderData()
        {
            //Debug.Log("Updating render data...");
            if (_grid == null)
            {
                Debug.LogWarning("Grid is null.");
                return;
            }

            if (_dirtyTiles.Count == 0)
            {
                Debug.LogWarning("No dirty tiles to update.");
            }

            
            // Update the matrices and uvOffsets for all dirty tiles
            foreach (int index in _dirtyTiles)
            {
                int x = index / _gridHeight;
                int y = index % _gridHeight;

                //Debug.Log($"Updating tile at ({x}, {y}) with index {index}");

                // Get the TileObject at this position
                TileObject tileObject = _grid.GetGridObject(x, y);
                
                // Get the TileType of the TileObject
                TileType tileType = tileObject.TileType;

                //if the change is invalid, discard the change
                if (tileType == TileType.None)
                {
                    // Clear the matrix and uvOffset for empty tiles
                    _matrices[index] = Matrix4x4.identity;
                    _uvOffsets[index] = Vector4.zero;
                    continue;
                }

                // Get the sprite for this tile type from the SpriteAtlas
                Sprite sprite = GetSpriteForTileType(tileType);

                if (sprite == null)
                {
                    Debug.LogWarning($"Sprite for TileType {tileType} not found in SpriteAtlas.");
                    continue;
                }

                // Calculate UV offsets
                Vector4 uvOffset = GetUVOffset(sprite);

                // Create the transformation matrix
                Vector3 position = _grid.GetWorldPosition(x, y) + new Vector3(_grid.GetCellSize(), _grid.GetCellSize()) * 0.5f;
                Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * _grid.GetCellSize());

                _matrices[index] = matrix;
                _uvOffsets[index] = uvOffset;
            }

            // Clear the dirty tiles set after updating
            _dirtyTiles.Clear();
        }

        
        /**
         * Draws the tiles on the screen
         * Runs every frame to render the tiles
         * Uses Graphics.DrawMeshInstanced to render all tiles in a single draw call
         */
        private void RenderTiles()
        {
            if (_matrices == null || _uvOffsets == null)
            {
                Debug.LogWarning("Matrices or UVOffsets are null.");
                return;
            }

            int totalTiles = _matrices.Length;
            int batchSize = 1023;

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            int batchCount = 0;
            int renderedTiles = 0;

            for (int i = 0; i < totalTiles; i++)
            {
                if (_uvOffsets[i] == Vector4.zero)
                    continue; // Skip empty tiles

                _batchMatricesArray[batchCount] = _matrices[i];
                _batchUVOffsetsArray[batchCount] = _uvOffsets[i];
                batchCount++;

                // When batch is full or at the end, render it
                if (batchCount == batchSize || i == totalTiles - 1)
                {
                    props.Clear();
                    props.SetVectorArray("_UVOffset", _batchUVOffsetsArray);

                    Graphics.DrawMeshInstanced(tileMesh, 0, baseTileMaterial, _batchMatricesArray, batchCount, props);

                    batchCount = 0;
                }
            }
            
            
            //Debug.Log($"Total tiles rendered: {renderedTiles}");
        }

        private Sprite GetSpriteForTileType(TileType tileType)
        {
            // Assuming the sprite names correspond to the TileType names
            string spriteName = "T_" + tileType + "_0";
            return spriteAtlas.GetSprite(spriteName);
        }

        private Vector4 GetUVOffset(Sprite sprite)
        {
            Rect textureRect = sprite.textureRect;
            Texture atlasTexture = sprite.texture;

            float atlasWidth = atlasTexture.width;
            float atlasHeight = atlasTexture.height;

            Vector2 uvOffset = new Vector2(textureRect.xMin / atlasWidth, textureRect.yMin / atlasHeight);
            Vector2 uvScale = new Vector2(textureRect.width / atlasWidth, textureRect.height / atlasHeight);

            return new Vector4(uvOffset.x, uvOffset.y, uvScale.x, uvScale.y);
        }
    }
}
