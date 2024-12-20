using _Script.Damageable;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Items.Lootable;
using UnityEngine;

namespace _Script.Interactable.WorldResources
{
    public class Ore : MonoBehaviour, IDamageable
    {
        
        [SerializeField] private float health = 1;
        //set tag to "Ore"
        private void Start()
        {
            gameObject.tag = "Ore";
        }
        
        public float ApplyDamage(float damage)
        {
            health -= damage;
            if (health <= 0)
            {
                Die();
            }
            return damage;
        }
        
        private void Die()
        {
            Destroy(gameObject);
        }
    }
}