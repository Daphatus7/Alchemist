using UnityEngine;

namespace _Script.Character
{
    public interface IControl
    {
        public void LeftMouseButton(Vector2 direction);
        public void RightMouseButton(Vector2 direction);
        public void SpaceBar(Vector2 direction);
    }
}