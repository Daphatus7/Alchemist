// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 08

using _Script.Map.MapLoadContext.ContextInstance;
using UnityEngine;

namespace _Script.Map.MapLoadContext.Scriptable
{
    [CreateAssetMenu(fileName = "MapLoadContext", menuName = "MapLoadContext/MapLoadContext")]
    public abstract class MapLoadContext : ScriptableObject
    {
        public string mapName; 
        public abstract MapType MapType { get; }
    }
    
    public enum MapType
    {
        Monster,
        Boss,
        Supply
    }
}