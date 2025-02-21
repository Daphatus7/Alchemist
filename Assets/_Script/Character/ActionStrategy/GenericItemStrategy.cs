using _Script.Inventory.ItemInstance;
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
            if (currentUseItem is null or null) return false;

            // If the item is a seed, show seed preview on fertile ground
            if (currentUseItem.ItemInstance.ItemTypeString == "Seed")
            {
                if (currentUseItem.ItemInstance is not SeedInstance seedItem) return false;

                var tile = GameTileMap.PointedTile;
                if (tile != null && tile.IsFertile)
                {
                    currentSpriteRenderer.sprite = seedItem.SeedOnGroundSprite;
                    currentItem.transform.position = GameTileMap.Instance.GetTileWorldCenterPosition(tile.Position.x, tile.Position.y);
                    return true;
                }
            }

            return false;
        }
    }
}