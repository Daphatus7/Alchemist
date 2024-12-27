// Author : Peiyu Wang @ Daphatus
// 20 12 2024 12 25

using _Script.Drop.DropTable;
using _Script.Items.Lootable;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Script.Drop
{
    public class DropItemComponent : MonoBehaviour
    {
        [SerializeField] private ScriptableObject dropProviderObject; // Assign a DropTable asset implementing IDropProvider
    
        private IDropProvider _dropProvider;

        private void Start()
        {
            // Convert the ScriptableObject to IDropProvider
            _dropProvider = dropProviderObject as IDropProvider;
        }
        
        public void DropItems()
        {
            Drop(transform.position);
        }
        
        protected virtual void Drop(Vector3 pos)
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
                        itemObj.transform.position = pos + new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);
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