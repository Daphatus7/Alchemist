using System.Collections.Generic;
using _Script.Inventory.PlayerInventory;
using _Script.Items.AbstractItemTypes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.ActionStrategy
{
    public sealed class WeaponStrategy : MonoBehaviour, IActionStrategy
    {
        [SerializeField] private GameObject weaponSlot;
        private Weapon.Weapon _currentWeapon;
        [SerializeField] private List<string> targetTags;

        private void Awake()
        {
            targetTags = new List<string> {"Enemy"};
            _currentWeapon = GetComponentInChildren<Weapon.Weapon>();
            _currentWeapon?.SetTargetType(targetTags);
        }

        private void Update()
        {
            if (_currentWeapon)
            {
                RotateWeaponTowardsMouse();
            }
        }

        private void RotateWeaponTowardsMouse()
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

        public void LeftMouseButtonDown(Vector3 direction)
        {
            Pressed(direction);
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
            Released(direction);
        }

        public void Pressed(Vector2 direction)
        {
            if(_currentWeapon != null && !_currentWeapon.IsCoolingDown)
            {
                _currentWeapon.OnPressed(direction);
            }
        }

        public void Released(Vector2 direction)
        {
            _currentWeapon?.OnReleased(direction);
        }

        public void ChangeItem(ActionBarContext useItem)
        {
            var weaponItem = useItem.ItemData as WeaponItem;
            if (weaponItem != null)
            {
                // If there's already a weapon equipped, prevent change
                if (_currentWeapon)
                {
                    Debug.LogError("Cannot change weapon, there is already a weapon equipped");
                    return;
                }

                var weaponPrefab = weaponItem.weaponPrefab;
                var weapon = Instantiate(weaponPrefab, weaponSlot.transform.position, Quaternion.identity, weaponSlot.transform);
                if (_currentWeapon != null)
                {
                    Destroy(_currentWeapon.gameObject);
                }
                _currentWeapon = weapon.GetComponent<Weapon.Weapon>();
                _currentWeapon.SetWeaponItem(weaponItem, targetTags);
            }
        }

        public void RemoveItem()
        {
            if(_currentWeapon)
            {
                Destroy(_currentWeapon.gameObject);
                _currentWeapon = null;
            }
        }
    }
}
