// Author : Peiyu Wang @ Daphatus
// 15 02 2025 02 36

using _Script.Movement;
using UnityEngine;

namespace _Script.Enemy.EnemyAbility
{
    public class EnemyRangedAttack : EnemyAttack
    {
        [SerializeField] private GameObject damagePrefab;
        public override void UseAbility(Transform target)
        {
            //shoot projectile
            var projectile = Instantiate(damagePrefab, transform.position, Quaternion.identity);
            var c = projectile.GetComponent<Projectile>();
            c.Fire((target.position - transform.position).normalized);
        }
    }
}