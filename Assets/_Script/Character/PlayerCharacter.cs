using _Script.Attribute;
using _Script.Character.Ability;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character
{
    public class PlayerCharacter : PlayerAttribute, IControl
    {
        [SerializeField] private GameObject LeftHand;
        [SerializeField] private GameObject RightHand;
        private AttackAbility _attackAbility;
        private Vector3 _CursorPosition; public Vector3 CursorPosition => _CursorPosition;
        private float _mouseAngle; public float MouseAngle => _mouseAngle;
        private float _facingDirection; public float FacingDirection => _facingDirection;

        private void Awake()
        {
            _attackAbility = GetComponent<AttackAbility>();
        }


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
            _attackAbility.Shoot(transform, direction);
        }

        public void NormalAttack(Vector2 direction)
        {
            var facingLeft = Helper.IsFaceLeft(_mouseAngle);
            if(facingLeft)
            {
                _attackAbility.NormalAttack(LeftHand.transform, direction);
            }
            else
            {
                _attackAbility.NormalAttack(RightHand.transform, direction);
            }
        }


        public void LeftMouseButton(Vector2 direction)
        {
            NormalAttack(direction);
        }

        public void RightMouseButton(Vector2 direction)
        {
            Shoot(direction);
        }

        public void SpaceBar(Vector2 direction)
        {
            throw new System.NotImplementedException();
        }
    }
}