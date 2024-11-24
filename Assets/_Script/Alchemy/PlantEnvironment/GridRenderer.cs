using System;
using System.Collections.Generic;
using _Script.Alchemy.PlantEnvironment;
using UnityEngine;

[ExecuteInEditMode]
public class GridRenderer : MonoBehaviour
{
    [SerializeField] private Mesh tileMesh; // a quad mesh
    [SerializeField] private Material baseTileMaterial; // a material with a shader that supports instancing

    // create a texture for each tile type
    [SerializeField] private Texture2D groundTexture;
    [SerializeField] private Texture2D pathTexture;
    [SerializeField] private Texture2D dirtTexture;
    [SerializeField] private Texture2D wetDirtTexture;

    private Grid<TileObject> _grid; public Grid<TileObject> Grid => _grid;
    private Dictionary<TileType, List<Matrix4x4>> _tileTypeMatrices = new Dictionary<TileType, List<Matrix4x4>>();
    private Dictionary<TileType, Material> _tileTypeMaterials = new Dictionary<TileType, Material>();

    private bool _needsUpdate = true;

    private void Awake()
    {
        // create a material for each tile type

    }

    private void Start()
    {
        InitializeMaterials();
    }

    public void InitializeMaterials()
    {
        CreateMaterialForTileType(TileType.Ground, groundTexture);
        CreateMaterialForTileType(TileType.Path, pathTexture);
        CreateMaterialForTileType(TileType.Dirt, dirtTexture);
        CreateMaterialForTileType(TileType.WetDirt, wetDirtTexture);
    }
    


    private void CreateMaterialForTileType(TileType tileType, Texture2D texture)
    {
        if (texture != null)
        {
            Material material = new Material(baseTileMaterial);
            material.mainTexture = texture;
            material.enableInstancing = true;
            _tileTypeMaterials[tileType] = material;
        }
        else
        {
            Debug.LogWarning($"Texture for TileType {tileType} is null.");
        }
    }

    public void SetGrid(Grid<TileObject> grid)
    {
        _grid = grid;
        _grid.OnGridValueChanged += OnGridValueChanged;
        UpdateRenderData();
    }

    private void OnDestroy()
    {
        if (_grid != null)
        {
            _grid.OnGridValueChanged -= OnGridValueChanged;
        }
    }

    private void OnGridValueChanged(object sender, Grid<TileObject>.OnGridValueChangedEventArgs e)
    {
        _needsUpdate = true;
    }

    private void LateUpdate()
    {   

        
        if (_needsUpdate)
        {
            UpdateRenderData();
            _needsUpdate = false;
        }

        RenderTiles();
    }

    private void UpdateRenderData()
    {
        if(_tileTypeMatrices == null)
        {
            Debug.LogError("_tileTypeMatrices is null.");
            return;
        }
        _tileTypeMatrices.Clear();

        if (_grid == null)
        {
            Debug.LogWarning("Grid is null.");
            return;
        }

        for (int x = 0; x < _grid.GetWidth(); x++)
        {
            for (int y = 0; y < _grid.GetHeight(); y++)
            {
                TileObject tileObject = _grid.GetGridObject(x, y);
                TileType tileType = tileObject.TileType;

                if (tileType == TileType.None)
                    continue;

                if (!_tileTypeMatrices.ContainsKey(tileType))
                {
                    _tileTypeMatrices[tileType] = new List<Matrix4x4>();
                }

                Vector3 position = _grid.GetWorldPosition(x, y) + new Vector3(_grid.GetCellSize(), _grid.GetCellSize()) * 0.5f;
                Matrix4x4 matrix = Matrix4x4.TRS(position, Quaternion.identity, Vector3.one * _grid.GetCellSize());
                _tileTypeMatrices[tileType].Add(matrix);
            }
        }
    }

    private void RenderTiles()
    {
        foreach (var kvp in _tileTypeMatrices)
        {
            TileType tileType = kvp.Key;
            List<Matrix4x4> matrices = kvp.Value;

            // get the material for this tile type
            if (!_tileTypeMaterials.TryGetValue(tileType, out Material tileMaterial))
            {
                Debug.LogWarning($"Material for TileType {tileType} not found.");
                continue; // skip this tile type if the material is not found
            }

            int count = matrices.Count;
            int batchSize = 1023;
            for (int i = 0; i < count; i += batchSize)
            {
                int len = Mathf.Min(batchSize, count - i);
                Graphics.DrawMeshInstanced(tileMesh, 0, tileMaterial, matrices.GetRange(i, len).ToArray(), len);
            }
        }
    }
}
