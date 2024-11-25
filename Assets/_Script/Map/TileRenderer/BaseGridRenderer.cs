using System.Collections.Generic;
using _Script.Alchemy.PlantEnvironment;
using _Script.Map.GridMap;
using UnityEngine;
using UnityEngine.U2D;

namespace _Script.Map.TileRenderer
{
    [ExecuteInEditMode]
    public abstract class BaseGridRenderer<TGridObject> : MonoBehaviour
    {
        [SerializeField] protected Mesh mesh; // The mesh to render
        [SerializeField] protected Material baseMaterial; // Material with a shader that supports instancing

        [SerializeField] protected SpriteAtlas spriteAtlas; // The SpriteAtlas containing all sprites

        protected Grid<TGridObject> _grid;
        public Grid<TGridObject> Grid => _grid;

        protected Matrix4x4[] _matrices;
        protected Vector4[] _uvOffsets;
        protected readonly HashSet<int> _dirtyTiles = new HashSet<int>();
        protected int _gridWidth;
        protected int _gridHeight;

        protected readonly Matrix4x4[] _batchMatricesArray = new Matrix4x4[1023];
        protected readonly Vector4[] _batchUVOffsetsArray = new Vector4[1023];

        protected bool _needsUpdate = true;

        protected virtual void Awake()
        {
            if (spriteAtlas == null)
            {
                Debug.LogError($"{nameof(spriteAtlas)} is not assigned in {GetType().Name}.");
                return;
            }

            if (baseMaterial == null)
            {
                Debug.LogError($"{nameof(baseMaterial)} is not assigned in {GetType().Name}.");
                return;
            }

            // Assign the atlas texture to the material
            baseMaterial.mainTexture = GetAtlasTexture();
        }

        public void SetGrid(Grid<TGridObject> grid)
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

                // Mark all tiles as dirty at the start
                _dirtyTiles.Add(i);
            }

            // Subscribe to the grid's value changed event
            SubscribeToGridEvents();

            _needsUpdate = true;
        }

        protected virtual void OnDestroy()
        {
            UnsubscribeFromGridEvents();
        }

        protected virtual void LateUpdate()
        {
            if (_needsUpdate)
            {
                UpdateRenderData();
                _needsUpdate = false;
            }

            RenderInstances();
        }

        protected void RenderInstances()
        {
            if (_matrices == null || _uvOffsets == null)
            {
                Debug.LogWarning("Matrices or UVOffsets are null.");
                return;
            }

            int totalInstances = _matrices.Length;
            int batchSize = 1023;

            MaterialPropertyBlock props = new MaterialPropertyBlock();
            int batchCount = 0;

            for (int i = 0; i < totalInstances; i++)
            {
                if (_uvOffsets[i] == Vector4.zero)
                    continue; // Skip empty instances

                _batchMatricesArray[batchCount] = _matrices[i];
                _batchUVOffsetsArray[batchCount] = _uvOffsets[i];
                batchCount++;

                // When batch is full or at the end, render it
                if (batchCount == batchSize || i == totalInstances - 1)
                {
                    props.Clear();
                    props.SetVectorArray("_UVOffset", _batchUVOffsetsArray);

                    Graphics.DrawMeshInstanced(mesh, 0, baseMaterial, _batchMatricesArray, batchCount, props);

                    batchCount = 0;
                }
            }
        }

        protected abstract void SubscribeToGridEvents();

        protected abstract void UnsubscribeFromGridEvents();

        protected virtual void UpdateRenderData()
        {
            throw new System.NotImplementedException();
        }

        protected abstract Texture GetAtlasTexture();

        protected abstract int GetIndex(int x, int y);

        protected abstract Vector3 GetInstancePosition(int x, int y);

        protected abstract Vector4 GetUVOffset(TGridObject gridObject);

        protected abstract bool ShouldRenderInstance(TGridObject gridObject);
    }
}
