using UnityEngine;

namespace _Script.Character
{
    public interface IControl
    {
        public void LeftMouseButtonDown(Vector2 direction);
        public void RightMouseButtonDown(Vector2 direction);
        public void LeftMouseButtonUp(Vector2 direction);
        public void RightMouseButtonUp(Vector2 direction);
        public void Move(Vector2 direction);
        public void Dash(Vector2 direction);
        public void Sprint(Vector2 direction);
        public void SprintEnd(Vector2 direction);
    }
}