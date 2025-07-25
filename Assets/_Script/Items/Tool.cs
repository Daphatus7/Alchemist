using _Script.Character;
using _Script.Items.AbstractItemTypes;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Tool", menuName = "Items/Equipments/Weapons/Tool")]
    public class Tool : WeaponItem
    {
        public ToolType toolType = ToolType.None;
        public float durability = 1;
    }
    
    public enum ToolType
    {
        Chop,
        Mine,
        None,
    }
}