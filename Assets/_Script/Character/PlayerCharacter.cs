using _Script.Attribute;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    public class PlayerCharacter : PlayerAttribute
    {
        
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject normalAttackPrefab;

        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;

        private Vector3 _CursorPosition; public Vector3 CursorPosition => _CursorPosition;
        private float _mouseAngle; public float MouseAngle => _mouseAngle;
        private float _facingDirection; public float FacingDirection => _facingDirection;
        
        
        private void Update()
        {
            UpdateCursorPosition();
        }
        
        private void UpdateCursorPosition()
        {
            _CursorPosition = Camera.main!.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            _mouseAngle = Mathf.Atan2(_CursorPosition.y - transform.position.y, _CursorPosition.x - transform.position.x) * Mathf.Rad2Deg;
        }
        
        
        public void Shoot(Vector2 direction)
        {
            Quaternion shootDirection = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            Instantiate(projectilePrefab, transform.position, shootDirection);
        }

        public void NormalAttack(Vector2 direction)
        {
            Quaternion shootDirection = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            //attach the normal attack prefab to the player
            var facingLeft = Helper.IsFaceLeft(_mouseAngle);
            if(facingLeft)
            {
                Instantiate(normalAttackPrefab, LeftHand.transform.position, shootDirection, LeftHand.transform);
            }
            else
            {
                Instantiate(normalAttackPrefab, RightHand.transform.position, shootDirection, RightHand.transform);
            }

        }
    }
}