// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 19

using _Script.Character;
using _Script.Items.AbstractItemTypes;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [CreateAssetMenu(fileName = "New Torch Item", menuName = "Items/Torch Item")]
    public class TorchItem : EquipmentItem
    {
        [SerializeField] private float torchDuration = 10f;
        public override EquipmentType EquipmentType => EquipmentType.Torch;
        public override ItemType ItemType => ItemType.Torch;
        public override string ItemTypeString => "Torch";

        public override bool Use(PlayerCharacter playerCharacter)
        {
            return true;
        }
    }
}