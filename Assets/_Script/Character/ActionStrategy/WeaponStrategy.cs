using System.Collections.Generic;
using _Script.Items.AbstractItemTypes;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.ActionStrategy
{
    public sealed class WeaponStrategy : MonoBehaviour, IActionStrategy
    {
        [SerializeField] private GameObject weaponSlot;
        
        private Weapon.Weapon _currentWeapon;
        
        [Header("Attacks")]
        //valid target tags
        [SerializeField]
        private List<string> targetTags;
        
        private void Awake()
        {
            targetTags = new List<string> {"Enemy"};
            _currentWeapon = GetComponentInChildren<Weapon.Weapon>();
            Debug.LogWarning("Set target tags manually, should move to when weapon is equipped");
            _currentWeapon?.SetTargetType(targetTags);
        }
        
        private void Update()
        {
            // if has a weapon, rotate the weapon to face the mouse
            if(_currentWeapon)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                var fireDirection = Vector2.zero;
                if (Camera.main)
                {
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    fireDirection = (worldPosition - transform.position).normalized;
                }
                var angle = Vector2.SignedAngle(Vector2.up, fireDirection);
                _currentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        /// <summary>
        /// Make sure there is no weapon selected before changing weapon
        /// </summary>
        /// <param name="itemData"></param>
        public void ChangeWeapon(ItemData itemData)
        {
            var weaponItem = itemData as WeaponItem;
            if (weaponItem != null)
            {
                var weaponPrefab = weaponItem.weaponPrefab;
            
                //if there is a weapon, don't change weapon
                if (_currentWeapon)
                {
                    Debug.LogError("Cannot change weapon, there is a weapon equipped");
                    return;
                }
                // Spawn weapon
                var weapon = Instantiate(weaponPrefab, weaponSlot.transform.position, Quaternion.identity);
                weapon.transform.parent = weaponSlot.transform;
                // destroy current weapon
                if(_currentWeapon != null)
                {
                    Destroy(_currentWeapon.gameObject);
                }
            
                // set new weapon
                _currentWeapon = weapon.GetComponent<Weapon.Weapon>();
            }

            _currentWeapon.SetWeaponItem(weaponItem, targetTags);
        }
        
        public void RemoveWeapon()
        {
            if(_currentWeapon)
            {
                Destroy(_currentWeapon.gameObject);
                _currentWeapon = null;
            }
        }
        
        public void Pressed(Vector2 direction)
        {
            if(_currentWeapon != null)
            {
                if (!_currentWeapon.IsCoolingDown)
                {
                    _currentWeapon.OnPressed(direction);
                }
            }
        }
        
        public void Released(Vector2 direction)
        {
            if(_currentWeapon != null)
            {
                _currentWeapon.OnReleased(direction);
            }
        }

        public void LeftMouseButtonDown(Vector3 direction)
        {
            
            Pressed(direction);
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
            Released(direction);
        }
    }

}