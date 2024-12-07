using _Script.Items;
using _Script.Map;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    [DefaultExecutionOrder(50)]
    public sealed class GenericItemStrategy : BaseItemStrategy
    {
        protected override bool TryShowPreview()
        {
            if (currentUseItem == null || currentUseItem.ItemData == null) return false;

            // If the item is a seed, show seed preview on fertile ground
            if (currentUseItem.ItemData.ItemTypeString == "Seed")
            {
                var seedItem = currentUseItem.ItemData as SeedItem;
                if (seedItem == null) return false;

                var tile = GameTileMap.Instance.PointedTile;
                if (tile != null && tile.IsFertile)
                {
                    currentSpriteRenderer.sprite = seedItem.seedOnGroundSprite;
                    currentItem.transform.position = GameTileMap.Instance.GetTileWorldCenterPosition(tile.Position.x, tile.Position.y);
                    return true;
                }
            }

            return false;
        }
    }
}