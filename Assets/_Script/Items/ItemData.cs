using UnityEngine;
namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "Inventory System/Item")]
    public class ItemData : ScriptableObject
    {
        public int id;
        public string itemName;
        public string description;
        public Sprite icon;
        public int maxStackSize = 1;
    }
}