using System.Collections;
using UnityEngine;

namespace _Script.Movement
{
    public class Slash : Attack
    {

        protected override void UpdateRotation(float time)
        {
            if (AttackingLeft)
            {
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, 90, AnimationCurve.Evaluate(time / AttackTime))) * InitialRotation;
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, Mathf.Lerp(0, -90, AnimationCurve.Evaluate(time / AttackTime))) * InitialRotation;
            }
        }
        
        protected override void UpdatePosition(float time)
        {
        }
    }
}