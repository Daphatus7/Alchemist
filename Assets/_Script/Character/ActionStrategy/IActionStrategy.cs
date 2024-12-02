using System.Collections.Generic;
using _Script.Items.AbstractItemTypes;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.ActionStrategy
{
    public interface IActionStrategy
    {
        void LeftMouseButtonDown(Vector3 direction);
        void LeftMouseButtonUp(Vector3 direction);
    }
}