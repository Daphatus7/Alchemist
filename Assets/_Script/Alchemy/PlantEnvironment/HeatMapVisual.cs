using _Script.Utilities;
using UnityEngine;

namespace _Script.Alchemy.PlantEnvironment
{
    public class HeatMapVisual : MonoBehaviour 
    {

        private Grid<int> _grid;
        private Mesh _mesh;
        private bool _updateMesh;

        private void Awake() 
        {
            _mesh = new Mesh();
            GetComponent<MeshFilter>().mesh = _mesh;
        }

        public void SetGrid(Grid<int> grid) 
        {
            _grid = grid;
            UpdateHeatMapVisual();
            _grid.OnGridValueChanged += Grid_OnGridValueChanged;
        }

        private void Grid_OnGridValueChanged(object sender, Grid<int>.OnGridValueChangedEventArgs e)
        {
            //UpdateHeatMapVisual();
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

        private void UpdateHeatMapVisual() 
        {
            Helper.CreateEmptyMeshArrays(_grid.GetWidth() * _grid.GetHeight(), out Vector3[] vertices, out Vector2[] uv, out int[] triangles);

            for (int x = 0; x < _grid.GetWidth(); x++) 
            {
                for (int y = 0; y < _grid.GetHeight(); y++) 
                {
                    int index = x * _grid.GetHeight() + y;
                    Vector3 quadSize = new Vector3(1, 1) * _grid.GetCellSize();

                    int gridValue = _grid.GetValue(x, y);
                    float gridValueNormalized = (float)gridValue / 100;
                    Vector2 gridValueUV = new Vector2(gridValueNormalized, 0f);
                    Helper.AddToMeshArrays(vertices, uv, triangles, index, _grid.GetWorldPosition(x, y) + quadSize * .5f, 0f, quadSize, gridValueUV, gridValueUV);
                }
            }

            _mesh.vertices = vertices;
            _mesh.uv = uv;
            _mesh.triangles = triangles;
        }

    }
}
