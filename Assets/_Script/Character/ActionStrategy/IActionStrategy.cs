using _Script.Inventory.PlayerInventory;

namespace _Script.Character.ActionStrategy
{
    public interface IActionStrategy
    {
        void LeftMouseButtonDown(UnityEngine.Vector3 direction);
        void LeftMouseButtonUp(UnityEngine.Vector3 direction);
        void ChangeItem(ActionBarContext useItem);
        void RemoveItem();
    }
}