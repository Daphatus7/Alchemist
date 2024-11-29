using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Utilities
{
    public class CursorMovementTracker : Singleton<CursorMovementTracker>
    {
        private bool _hasCursorMoved = false; public bool HasCursorMoved => _hasCursorMoved;
        private Vector2 _lastCursorPosition;

        private void Start()
        {
            // Initialize cursor position
            _lastCursorPosition = Mouse.current.position.ReadValue();
        }

        private void Update()
        {
            TrackCursorMovement();
        }

        private void TrackCursorMovement()
        {
            // Get the current cursor position
            Vector2 currentCursorPosition = Mouse.current.position.ReadValue();

            // Check if the cursor has moved
            if (currentCursorPosition != _lastCursorPosition)
            {
                _hasCursorMoved = true;
                _lastCursorPosition = currentCursorPosition;
            }
            else
            {
                _hasCursorMoved = false;
            }
        }
    }
}