// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 08

using _Script.Map.MapLoadContext.ContextInstance;
using UnityEngine;

namespace _Script.Map.MapLoadContext.Scriptable
{
    public abstract class MapLoadContext : ScriptableObject
    {
        public string mapName; 
        public abstract MapType MapType { get; }
    }
    
    public enum MapType
    {
        Monster,
        Boss,
        Town
    }
}