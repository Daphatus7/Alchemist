using System.Collections.Generic;
using _Script.Attribute;
using _Script.Enemy.EnemyAbility;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Script.Enemy.EnemyCharacter
{
    public class EnemyCharacter : PawnAttribute
    {

        [SerializeField] private GameObject attackPrefab;
        private List<IEnemyAbilityHandler> _abilities;
        
        private void Start()
        {
            _abilities = new List<IEnemyAbilityHandler>(GetComponents<IEnemyAbilityHandler>());
        }
        
        protected override void OnDeath()
        {
            base.OnDeath();
            Destroy(gameObject);
        }
    }
}