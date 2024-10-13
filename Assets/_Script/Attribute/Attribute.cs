using UnityEngine;

namespace _Script.Attribute
{
    public class Attribute : MonoBehaviour
    {
        [SerializeField] protected float health = 100f; public float Health => health;
    }
}