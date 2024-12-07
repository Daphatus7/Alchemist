using System.Collections;
using System.Collections.Generic;
using _Script.Character.ActionStrategy;
using _Script.Inventory.ActionBarBackend;
using _Script.Items.AbstractItemTypes;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.Ability
{
    
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerAttack : MonoBehaviour, IActionStrategy
    {
        
        [Header("Weapon")]
        [SerializeField] private GameObject weaponSlot;
        [SerializeField] private Weapon.Weapon currentWeapon;
        
        [Header("Attacks")]
        //valid target tags
        [SerializeField] protected List<string> targetTags;
        
        private void Awake()
        {
            targetTags = new List<string> {"Enemy"};
            currentWeapon = GetComponentInChildren<Weapon.Weapon>();
            Debug.LogWarning("Set target tags manually, should move to when weapon is equipped");
            currentWeapon?.SetTargetType(targetTags);
        }
        
        private void Update()
        {
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
        
        public void ChangeWeapon(GameObject weaponPrefab, WeaponItem weaponItem)
        {
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
            if(currentWeapon != null)
            {
                Destroy(currentWeapon.gameObject);
            }
        }
        
        public virtual void Pressed(Vector2 direction)
        {
            if(currentWeapon != null)
            {
                if (!currentWeapon.IsCoolingDown)
                {
                    currentWeapon.OnPressed(direction);
                }
            }
        }
        
        public virtual void Released(Vector2 direction)
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

        public void ChangeItem(ActionBarContext useItem)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveItem()
        {
            throw new System.NotImplementedException();
        }
    }
}