// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 36

using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Map.MapManager
{
    public class RewardDataBase : ScriptableObject
    {
        public ItemData [] EquipmentRewards;
        public ItemData [] SupplyRewards;
    }
}