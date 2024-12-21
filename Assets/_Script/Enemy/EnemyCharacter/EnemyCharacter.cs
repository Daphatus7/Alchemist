using _Script.Attribute;
using _Script.Items.Drop;
using UnityEngine;

namespace _Script.Enemy.EnemyCharacter
{
    public class EnemyCharacter : PawnAttribute
    {
        [SerializeField] private GameObject attackPrefab;
    
        protected override void OnDeath()
        {
            GetComponent<DropItemComponent>()?.DropItems();
            Destroy(gameObject);
        }
    }
}