using System.Collections;
using System.Collections.Generic;
using _Script.Items.AbstractItemTypes;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.Ability
{
    [RequireComponent(typeof(PlayerCharacter))]
    public class PlayerAttack : MonoBehaviour
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
            if(currentWeapon != null)
            {
                Vector3 mousePosition = Mouse.current.position.ReadValue();
                var worldPosition = Vector3.zero;
                var _fireDirection = Vector2.zero;
                if (Camera.main != null)
                {
                    worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    _fireDirection = (worldPosition - transform.position).normalized;
                }
                var angle = Vector2.SignedAngle(Vector2.up, _fireDirection);
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
        
        public void LootWeapon(Weapon.Weapon weapon)
        {
            if(currentWeapon != null)
            {
                Debug.Log("Drop weapon");
            }
            currentWeapon = weapon;
        }
    }
}