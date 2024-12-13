using System.Collections.Generic;
using _Script.Attribute;
using _Script.Enemy.DropTable;
using _Script.Enemy.EnemyAbility;
using _Script.Items.Lootable;
using UnityEngine;
using UnityEngine.Serialization;

namespace _Script.Enemy.EnemyCharacter
{
    public class EnemyCharacter : PawnAttribute
    {
        [SerializeField] private GameObject attackPrefab;
        [SerializeField] private ScriptableObject dropProviderObject; // Assign a DropTable asset implementing IDropProvider
    
        private IDropProvider _dropProvider;

        private void Start()
        {
            // Convert the ScriptableObject to IDropProvider
            _dropProvider = dropProviderObject as IDropProvider;
        }
    
        protected override void OnDeath()
        {
            DropItems();
            Destroy(gameObject);
        }
    
        private void DropItems()
        {
            if (_dropProvider == null) return;

            foreach (var drop in _dropProvider.GetDrops())
            {
                float roll = Random.value;

                if (roll <= drop.dropChance)
                {
                    int amount = Random.Range(drop.minAmount, drop.maxAmount + 1);
                    for (int i = 0; i < amount; i++)
                    {
                        //create a game object at the enemy's position
                        GameObject itemObj = new GameObject("DroppedItem");
                        itemObj.transform.position = transform.position;
                        var loot = itemObj.AddComponent<ItemLootable>();
                        var co = itemObj.AddComponent<BoxCollider2D>();
                        var sr = itemObj.AddComponent<SpriteRenderer>();
                        loot.Initialize(co, sr, drop.item, 1);     
                    }
                }
            }
        }
    }
}