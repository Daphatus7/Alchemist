// Author : Peiyu Wang @ Daphatus
// 07 02 2025 02 41

using System.Collections.Generic;
using _Script.UserInterface;
using UnityEngine;

namespace _Script.Managers
{
    [DefaultExecutionOrder(-1000)]
    public class UIManager : PersistentSingleton<UIManager>
    {
        private readonly Dictionary<UIType, IUIHandler> _uiHandlers = new Dictionary<UIType, IUIHandler>(); 
        public Dictionary<UIType, IUIHandler> UIHandlers => _uiHandlers;
        
    }

    public enum UIType
    {
        PlayerInventoryUI,
        AlchemyUI,
    }
}