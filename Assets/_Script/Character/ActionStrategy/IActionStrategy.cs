namespace _Script.Character.ActionStrategy
{
    public interface IActionStrategy
    {
        void LeftMouseButtonDown(UnityEngine.Vector3 direction);
        void LeftMouseButtonUp(UnityEngine.Vector3 direction);
        void ChangeItem(_Script.Inventory.ActionBarBackend.ActionBarContext useItem);
        void RemoveItem();
    }
}