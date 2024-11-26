using _Script.Map.GridMap;
using _Script.Map.Tile.Tile_Base;
using _Script.Map.TileRenderer;
using UnityEngine;

namespace _Script.Map.TileRenderer
{
    public class TileGridRenderer : BaseGridRenderer<BaseTile>
    {
        protected override void SubscribeToGridEvents()
        {
            _grid.OnGridValueChanged += OnGridValueChanged;
        }

        protected override void UnsubscribeFromGridEvents()
        {
            if (_grid != null)
            {
                _grid.OnGridValueChanged -= OnGridValueChanged;
            }
        }

        private void OnGridValueChanged(object sender, Grid<BaseTile>.OnGridValueChangedEventArgs e)
        {
            int index = GetIndex(e.x, e.y);
            _dirtyTiles.Add(index);
            _needsUpdate = true;
        }

        protected override void UpdateRenderData()
        {
            if (_grid == null)
            {
                Debug.LogWarning("Grid is null.");
                return;
            }

            foreach (int index in _dirtyTiles)
            {
                int x = index / _gridHeight;
                int y = index % _gridHeight;

                BaseTile tileObject = _grid.GetGridObject(x, y);

                if (!ShouldRenderInstance(tileObject))
                {
                    // Clear the matrix and uvOffset for empty tiles
                    _matrices[index] = Matrix4x4.identity;
                    _uvOffsets[index] = Vector4.zero;
                    continue;
                }

                // Get UV offset
                Vector4 uvOffset = GetUVOffset(tileObject);

                // Create the transformation matrix
                Matrix4x4 matrix = Matrix4x4.TRS(GetInstancePosition(x, y), Quaternion.identity, Vector3.one * _grid.GetCellSize());

                _matrices[index] = matrix;
                _uvOffsets[index] = uvOffset;
            }
            // Clear the dirty tiles set after updating
            _dirtyTiles.Clear();
        }

        protected override Texture GetAtlasTexture()
        {
            // Use any sprite's texture from the atlas
            Sprite sprite = spriteAtlas.GetSprite("T_Grass_0");
            return sprite?.texture;
        }

        protected override int GetIndex(int x, int y)
        {
            return x * _gridHeight + y;
        }

        protected override Vector3 GetInstancePosition(int x, int y)
        {
            return _grid.GetWorldPosition(x, y) + new Vector3(_grid.GetCellSize(), _grid.GetCellSize()) * 0.5f;
        }

        protected override Vector4 GetUVOffset(BaseTile tile)
        {
            TileType tileType = tile.GetTileType();
            Sprite sprite = GetSpriteForTileType(tileType);

            if (sprite == null)
            {
                Debug.LogWarning($"Sprite for TileType {tileType} not found in SpriteAtlas.");
                return Vector4.zero;
            }

            return CalculateUVOffset(sprite);
        }

        protected override bool ShouldRenderInstance(BaseTile tileObject)
        {
            return tileObject.GetTileType() != TileType.None;
        }

        private Sprite GetSpriteForTileType(TileType tileType)
        {
            string spriteName = "T_" + tileType + "_0";
            return spriteAtlas.GetSprite(spriteName);
        }

        private Vector4 CalculateUVOffset(Sprite sprite)
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
