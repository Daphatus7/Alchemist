// Author : Peiyu Wang @ Daphatus
// 05 12 2024 12 30

using UnityEngine;

namespace _Script.Enemy.EnemyAbility
{
    public interface IEnemyAbilityHandler
    {
       void UseAbility(Transform target);
    }
}