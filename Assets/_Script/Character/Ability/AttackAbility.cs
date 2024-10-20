using System;
using System.Collections.Generic;
using _Script.Damageable;
using UnityEngine;
using Object = UnityEngine.Object;

namespace _Script.Character.Ability
{
    public class AttackAbility : Ability
    {
        [SerializeField] private GameObject normalAttackPrefab;
        [SerializeField] private GameObject shootPrefab;
        [SerializeField] protected List<string> targetTags;

        private void Awake()
        {
            targetTags = new List<string> {"Player"};
        }

        public virtual void NormalAttack(Transform center, Vector2 direction)
        {
            var o = Object.Instantiate(normalAttackPrefab, center.position, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), center);
            o.GetComponent<Damage>().SetTargetType(targetTags);
        }
        
        public virtual void Shoot(Transform center, Vector2 direction)
        {
            var o = Object.Instantiate(shootPrefab, center.position, Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg), center);
            o.GetComponent<Damage>().SetTargetType(targetTags);
        }
    }
}