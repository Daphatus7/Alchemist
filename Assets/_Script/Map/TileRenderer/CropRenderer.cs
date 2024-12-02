// using _Script.Alchemy.PlantEnvironment;
// using UnityEngine;
// using UnityEngine.U2D;
//
// namespace _Script.TileRenderer
// {
//     public class CropGridRenderer : BaseGridRenderer<TileObject>
//     {
//         [SerializeField] private SpriteAtlas cropSpriteAtlas; // Separate atlas for crops
//
//         protected override void SubscribeToGridEvents()
//         {
//             _grid.OnGridValueChanged += OnGridValueChanged;
//         }
//
//         protected override void UnsubscribeFromGridEvents()
//         {
//             if (_grid != null)
//             {
//                 _grid.OnGridValueChanged -= OnGridValueChanged;
//             }
//         }
//
//         private void OnGridValueChanged(object sender, Grid<TileObject>.OnGridValueChangedEventArgs e)
//         {
//             int index = GetIndex(e.x, e.y);
//             _dirtyTiles.Add(index);
//             _needsUpdate = true;
//         }
//
//         protected override void UpdateRenderData()
//         {
//             base.UpdateRenderData();
//         }
//
//         protected override Texture GetAtlasTexture()
//         {
//             // Use any sprite's texture from the crop atlas
//             Sprite sprite = cropSpriteAtlas.GetSprite("CropStage_0");
//             return sprite?.texture;
//         }
//
//         protected override int GetIndex(int x, int y)
//         {
//             return x * _gridHeight + y;
//         }
//
//         protected override Vector3 GetInstancePosition(int x, int y)
//         {
//             return _grid.GetWorldPosition(x, y) + new Vector3(_grid.GetCellSize(), _grid.GetCellSize()) * 0.5f;
//         }
//
//         protected override Vector4 GetUVOffset(TileObject tileObject)
//         {
//             if (!tileObject.HasCrop)
//                 return Vector4.zero;
//
//             int growthStage = tileObject.CropGrowthStage;
//             Sprite sprite = GetSpriteForCropStage(growthStage);
//
//             if (sprite == null)
//             {
//                 Debug.LogWarning($"Sprite for crop stage {growthStage} not found in SpriteAtlas.");
//                 return Vector4.zero;
//             }
//
//             return CalculateUVOffset(sprite);
//         }
//
//         protected override bool ShouldRenderInstance(TileObject tileObject)
//         {
//             return tileObject.HasCrop;
//         }
//
//         private Sprite GetSpriteForCropStage(int growthStage)
//         {
//             string spriteName = $"CropStage_{growthStage}";
//             return cropSpriteAtlas.GetSprite(spriteName);
//         }
//
//         private Vector4 CalculateUVOffset(Sprite sprite)
//         {
//             // Same as in TileGridRenderer
//             Rect textureRect = sprite.textureRect;
//             Texture atlasTexture = sprite.texture;
//
//             float atlasWidth = atlasTexture.width;
//             float atlasHeight = atlasTexture.height;
//
//             Vector2 uvOffset = new Vector2(textureRect.xMin / atlasWidth, textureRect.yMin / atlasHeight);
//             Vector2 uvScale = new Vector2(textureRect.width / atlasWidth, textureRect.height / atlasHeight);
//
//             return new Vector4(uvOffset.x, uvOffset.y, uvScale.x, uvScale.y);
//         }
//     }
// }
