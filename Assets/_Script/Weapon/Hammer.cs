using UnityEngine;

namespace _Script.Weapon
{
    public class Hammer : MeleeWeapon
    {
        protected override void UpdateRotation(float time)
        {
            if (AttackingLeft)
            {
                var angle = Mathf.Lerp(-45, 45, AnimationCurve.Evaluate(time / AttackTime));
                transform.rotation = Quaternion.Euler(0, 0, angle) * InitialRotation;
            }
            else
            {
                var angle = Mathf.Lerp(45, -45, AnimationCurve.Evaluate(time / AttackTime));
                Debug.Log(angle);
                transform.rotation = Quaternion.Euler(0, 0, angle) * InitialRotation;
            }
        }
        protected override void UpdatePosition(float time, Vector2 direction)
        {
            
        }
    }
}