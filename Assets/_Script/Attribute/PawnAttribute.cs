using _Script.Damageable;

namespace _Script.Attribute
{
    public class PawnAttribute : Attribute, IDamageable
    {
        public float TakeDamage(float damage)
        {
            health -= damage;
            return damage;
        }
    }
}