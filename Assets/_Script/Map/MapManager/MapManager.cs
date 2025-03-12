// Author : Peiyu Wang @ Daphatus
// 12 03 2025 03 10

using System.Collections.Generic;
using _Script.Map.MapLoadContext;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.MapLoadContext.Scriptable;
using UnityEngine;

namespace _Script.Map.MapManager
{
    
    /// <summary>
    /// Holds the data for this game instance
    /// </summary>
    public class MapManager : PersistentSingleton<MapManager>
    {
        /// <summary>
        /// Map - [Map a] - [Map c] - [Map d] - [Map b] - [Map e]
        ///     - [Map b]           - [Map e]
        /// Multiple maps as options for each level
        /// </summary>
        private Queue<MapLoadContextInstance []> _currentMaps;
        
        [SerializeField] private BossMapLoadContext [] bossMaps;
        
        [SerializeField] private MonsterMapLoadContext [] monsterMaps;
        
        [SerializeField] private int miniMapCount = 3;
            
        public void GenerateGameMaps()
        {
            //First Round
            //Generate 
            for(var i = miniMapCount - 1; i >= 0; i--)
            {
                _currentMaps.Enqueue(GenerateRandomMiniMaps());
            }
        }

        public MapLoadContextInstance[] GenerateRandomMiniMaps()
        {
            //Generate 2 unique options
            
        }
    }
}