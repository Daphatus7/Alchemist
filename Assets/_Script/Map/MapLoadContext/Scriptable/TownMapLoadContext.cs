// Author : Peiyu Wang @ Daphatus
// 24 03 2025 03 28

using UnityEngine;

namespace _Script.Map.MapLoadContext.Scriptable
{
    [CreateAssetMenu(fileName = "TownMapLoadContext", menuName = "MapLoadContext/TownMapLoadContext")]
    public class TownMapLoadContext : MapLoadContext
    {
        public override MapType MapType => MapType.Town;
        
    }
}
