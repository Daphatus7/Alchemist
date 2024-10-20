using _Script.Damageable;

namespace _Script.Attribute
{
    public class PawnAttribute : Attribute, IDamageable
    {
        public float ApplyDamage(float damage)
        {
            health -= damage;
            return damage;
        }
    }
}