using UnityEngine;

namespace _Script.Character
{
    public class PlayerCharacter : Pawn
    {
        
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private GameObject normalAttackPrefab;
        
        public void Shoot(Vector2 direction)
        {
            Quaternion shootDirection = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            Instantiate(projectilePrefab, transform.position, shootDirection);
        }

        public void NormalAttack(Vector2 direction)
        {
            Quaternion shootDirection = Quaternion.Euler(0, 0, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
            //attach the normal attack prefab to the player
            var n = Instantiate(normalAttackPrefab, transform.position, shootDirection);
            n.transform.SetParent(transform, true);
            
        }
    }
}