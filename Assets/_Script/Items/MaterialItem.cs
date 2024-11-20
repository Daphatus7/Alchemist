using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Items/Material")]
    public class MaterialItem : ItemData
    {
        public override ItemType ItemType => ItemType.Material;
        public override void Use(PlayerCharacter PlayerCharacter)
        {
            Debug.Log($"{ItemName} is a material and cannot be used directly.");
        }
    }
}