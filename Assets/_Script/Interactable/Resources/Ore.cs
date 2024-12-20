using _Script.Damageable;
using UnityEngine;

namespace _Script.Interactable.Resources
{
    public class Ore : MonoBehaviour, IDamageable
    {
        
        [SerializeField] private float health = 1;
        
        [SerializeField] private GameObject orePrefab;
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
            Debug.Log("Ore destroyed");
            Instantiate(orePrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}