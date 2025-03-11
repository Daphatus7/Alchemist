using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Map;
using _Script.Map.TileMap;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Seed Item", menuName = "Items/Material/Seed")]
    public class SeedItem : MaterialItem
    {
        public override ItemType ItemType => ItemType.Seed;
        public override string ItemTypeString => "Seed";
        public Sprite seedOnGroundSprite;
        
        //not instantiated reference to the prefab
        public GameObject cropPrefab;

        public override bool Use(PlayerCharacter playerCharacter)
        {
            return GameTileMap.Instance.AddCrop(cropPrefab);
        }
    }
}