using System;
using System.Collections.Generic;
using _Script.Utilities;
using UnityEngine;

namespace _Script.Map.GridMap
{
    /**
     * Only handles the logic component of the grid and holding the grid object
     */
    public class Grid<TGridObject>: IGridTileHandle
    {
        private int _width;
        public int GetWidth() => _width;
        private int _height;
        public int GetHeight() => _height;
        private float _cellSize;
        public float GetCellSize() => _cellSize;
        private TGridObject[,] _gridArray;
        private Vector3 _originPosition;
        private TextMesh[,] _debugTextArray;

        public bool IsDebug = false;

        public event EventHandler<OnGridValueChangedEventArgs> OnGridValueChanged;

        public class OnGridValueChangedEventArgs : EventArgs
        {
            public int x;
            public int y;
        }

        public Grid(int width, int height, float cellSize, Vector3 originPosition, Func<int, int, Grid<TGridObject>, TGridObject> createGridObject)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;

            _gridArray = new TGridObject[width, height];

            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = createGridObject(x, y, this);
                }
            }
            
            #region Debug
            if (IsDebug)
            {
                TextMesh[,] debugTextArray = new TextMesh[width, height];

                for (int x = 0; x < _gridArray.GetLength(0); x++)
                {
                    for (int y = 0; y < _gridArray.GetLength(1); y++)
                    {
                        debugTextArray[x, y] = Helper.CreateWorldText(_gridArray[x, y]?.ToString(), null,
                            GetWorldPosition(x, y) + new Vector3(cellSize, cellSize) * .5f, 30, Color.white,
                            TextAnchor.MiddleCenter);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.white, 100f);
                        Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.white, 100f);
                    }
                }

                Debug.DrawLine(GetWorldPosition(0, height), GetWorldPosition(width, height), Color.white, 100f);
                Debug.DrawLine(GetWorldPosition(width, 0), GetWorldPosition(width, height), Color.white, 100f);

                OnGridValueChanged += (object sender, OnGridValueChangedEventArgs eventArgs) =>
                {
                    debugTextArray[eventArgs.x, eventArgs.y].text = _gridArray[eventArgs.x, eventArgs.y]?.ToString();
                };
            }
            #endregion
        }
        
        public Grid(int width, int height, float cellSize, Vector3 originPosition)
        {
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _originPosition = originPosition;

            _gridArray = new TGridObject[width, height];
        }
        
        public void InitializeGrid(List<TGridObject> gridObjects)
        {
            for (int x = 0; x < _gridArray.GetLength(0); x++)
            {
                for (int y = 0; y < _gridArray.GetLength(1); y++)
                {
                    _gridArray[x, y] = gridObjects[x * _height + y];
                }
            }
        }

        public Vector3 GetWorldPosition(int x, int y)
        {
            return new Vector3(x, y) * _cellSize + _originPosition;
        }

        private void SetValue(int x, int y, TGridObject value)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                _gridArray[x, y] = value;
                _debugTextArray[x, y].text = _gridArray[x, y].ToString();
                OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }
        }

        public void UpdateTile(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
            }
        }


        // public void SetValue(Vector3 worldPosition, TGridObject value)
        // {
        //     int x, y;
        //     GetXY(worldPosition, out x, out y);
        //     SetValue(x, y, value);
        // }

        private void GetXY(Vector3 worldPosition, out int x, out int y)
        {
            x = Mathf.FloorToInt((worldPosition - _originPosition).x / _cellSize);
            y = Mathf.FloorToInt((worldPosition - _originPosition).y / _cellSize);
        }

        public TGridObject GetValue(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }
            else
            {
                return default;
            }
        }

        public TGridObject GetValue(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            return GetValue(x, y);
        }

        public TGridObject GetGridObject(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < _width && y < _height)
            {
                return _gridArray[x, y];
            }
            else
            {
                Debug.LogWarning("GridObject is null");
                return default(TGridObject);
            }
        }

        public TGridObject GetGridObject(Vector3 worldPosition)
        {
            int x, y;
            GetXY(worldPosition, out x, out y);
            return GetGridObject(x, y);
        }

        public void OnUpdate(int x, int y)
        {
            OnGridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { x = x, y = y });
        }
    }
}