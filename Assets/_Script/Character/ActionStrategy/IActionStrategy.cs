using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    public interface IActionStrategy
    {
        void LeftMouseButtonDown(Vector3 direction);
        void LeftMouseButtonUp(Vector3 direction);
    }
    
    public class WeaponStrategy : IActionStrategy
    {
        public void LeftMouseButtonDown(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
            throw new System.NotImplementedException();
        }
    }
}