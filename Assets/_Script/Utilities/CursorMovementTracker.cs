using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Utilities
{
    public class CursorMovementTracker : Singleton<CursorMovementTracker>
    {
        private static bool hasCursorMoved = false; public static bool HasCursorMoved => hasCursorMoved;
        private static Vector2 lastCursorPosition; public static Vector2 CursorPosition => lastCursorPosition;

        private void Start()
        {
            // Initialize cursor position
            lastCursorPosition = Helper.GetMouseWorldPosition();
        }

        private void Update()
        {
            TrackCursorMovement();
        }

        private void TrackCursorMovement()
        {
            // Get the current cursor position
            Vector2 currentCursorPosition = Helper.GetMouseWorldPosition();

            // Check if the cursor has moved
            if (currentCursorPosition != lastCursorPosition)
            {
                hasCursorMoved = true;
                lastCursorPosition = currentCursorPosition;
            }
            else
            {
                hasCursorMoved = false;
            }
        }
    }
}