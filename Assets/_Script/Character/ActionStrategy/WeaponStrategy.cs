using System.Collections.Generic;
using _Script.Items.AbstractItemTypes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.ActionStrategy
{
    public sealed class WeaponStrategy : MonoBehaviour, IActionStrategy
    {
        [SerializeField] private GameObject weaponSlot;
        
        private Weapon.Weapon currentWeapon;
        
        [Header("Attacks")]
        //valid target tags
        [SerializeField]
        private List<string> targetTags;
        
        private void Awake()
        {
            targetTags = new List<string> {"Enemy"};
            currentWeapon = GetComponentInChildren<Weapon.Weapon>();
            Debug.LogWarning("Set target tags manually, should move to when weapon is equipped");
            currentWeapon?.SetTargetType(targetTags);
        }
        
        private void Update()
        {
            // if has a weapon, rotate the weapon to face the mouse
            if(currentWeapon)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                var fireDirection = Vector2.zero;
                if (Camera.main)
                {
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    fireDirection = (worldPosition - transform.position).normalized;
                }
                var angle = Vector2.SignedAngle(Vector2.up, fireDirection);
                currentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }
        
        /// <summary>
        /// Make sure there is no weapon selected before changing weapon
        /// </summary>
        /// <param name="weaponPrefab"></param>
        /// <param name="weaponItem"></param>
        public void ChangeWeapon(GameObject weaponPrefab, WeaponItem weaponItem)
        {
            
            //if there is a weapon, don't change weapon
            if (currentWeapon)
            {
                Debug.LogError("Cannot change weapon, there is a weapon equipped");
                return;
            }
            // Spawn weapon
            var weapon = Instantiate(weaponPrefab, weaponSlot.transform.position, Quaternion.identity);
            weapon.transform.parent = weaponSlot.transform;
            // destroy current weapon
            if(currentWeapon != null)
            {
                Destroy(currentWeapon.gameObject);
            }
            
            // set new weapon
            currentWeapon = weapon.GetComponent<Weapon.Weapon>();
            currentWeapon.SetWeaponItem(weaponItem, targetTags);
        }
        
        public void RemoveWeapon()
        {
            if(currentWeapon)
            {
                Destroy(currentWeapon.gameObject);
                currentWeapon = null;
            }
        }
        
        public void Pressed(Vector2 direction)
        {
            if(currentWeapon != null)
            {
                if (!currentWeapon.IsCoolingDown)
                {
                    currentWeapon.OnPressed(direction);
                }
            }
        }
        
        public void Released(Vector2 direction)
        {
            if(currentWeapon != null)
            {
                currentWeapon.OnReleased(direction);
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