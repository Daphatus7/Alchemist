using _Script.Character;
using _Script.Items._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Material", menuName = "Items/Material")]
    public class MaterialItem : ItemData
    {
        public override void Use(PlayerCharacter playerCharacter)
        {
            Debug.Log($"{itemName} is a material and cannot be used directly.");
        }
    }
}