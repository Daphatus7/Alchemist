// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 18

using _Script.Inventory.PlayerInventory;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    public abstract class BaseActionStrategy : MonoBehaviour, IActionStrategy
    {
        public abstract void LeftMouseButtonDown(Vector3 direction);
        public abstract void LeftMouseButtonUp(Vector3 direction);
        public abstract void ChangeItem(ActionBarContext useItem);
        public abstract void RemoveItem();
    }
}

