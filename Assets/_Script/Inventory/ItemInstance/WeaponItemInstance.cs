// Author : Peiyu Wang @ Daphatus
// 21 02 2025 02 54

using System;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Inventory.ItemInstance
{
    public class WeaponItemInstance : EquipmentInstance
    {
        public WeaponItemInstance(ItemData itemData, bool rotated, int quantity = 1) : base(itemData, rotated, quantity)
        {
            CurrentDurability = ((WeaponItem) ItemData).durability;
        }

        #region Reading default data
        
        public float DamageMin => ((WeaponItem) ItemData).damageMin;
        public float DamageMax => ((WeaponItem) ItemData).damageMax;
        public float AttackCooldown => ((WeaponItem) ItemData).attackCooldown;
        public float AttackTime => ((WeaponItem) ItemData).attackTime;
        public AttackForm AttackForm => ((WeaponItem) ItemData).attackForm;
        public GameObject WeaponPrefab => ((WeaponItem) ItemData).weaponPrefab;
        public float AttackDistance => ((WeaponItem) ItemData).attackDistance;

        #endregion
        
        
        private int _currentDurability;
        public int CurrentDurability
        {
            get => _currentDurability;
            set
            {
                _currentDurability = value;
                if (_currentDurability <= 0)
                {
                    _currentDurability = 0;
                    Debug.Log("Weapon is broken");
                }
            }
        }

        public override ItemInstance Clone()
        {
            var clone = new WeaponItemInstance(ItemData, IsRotated, Quantity)
            {
                CurrentDurability = CurrentDurability
            };
            return clone;
        }
        
        public override ItemSave OnSaveData()
        {
            return new WeaponItemSave(this, CurrentDurability);
        }
    }
    
    [Serializable]
    public class WeaponItemSave : EquipmentItemSave
    {
        public WeaponItemSave(ItemInstance itemInstance, int currentDurability) : base(itemInstance)
        {
            this.currentDurability = currentDurability;
        }

        public int currentDurability;
        
        public override ItemInstance InitializeItem(ItemInstance itemInstance)
        {
            base.InitializeItem(itemInstance);
            if (itemInstance is WeaponItemInstance weaponItemInstance)
            {
                weaponItemInstance.CurrentDurability = currentDurability;
            }
            return itemInstance;
        }
    }
}