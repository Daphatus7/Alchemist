// Author : Peiyu Wang @ Daphatus
// 20 02 2025 02 42

using System;
using System.Collections.Generic;
using _Script.NPC.NpcBackend;
using _Script.Utilities.ServiceLocator;

namespace _Script.Managers
{
    public class TownManager : Singleton<TownManager>, ISaveTownDataHandler
    {
        private List<NpcBase> _npcs;
        public string SaveKey => "TownManager";
        
        /// <summary>
        /// Exit point for town Data
        /// </summary>
        /// <returns></returns>
        public object OnSaveData()
        {
            var npcHandler = new List<INpcSaveDataHandler>(_npcs);
            var nData = new Dictionary<string, NpcSave>();
            
            //Pack the Npc data
            foreach (var npc in npcHandler)
            {
                if(npc == null) continue;
                var npcSave = npc.OnSaveData();
                nData.Add(npc.SaveKey, npcSave);
            }
            
            //Pack the town data
            var townData = new TownData()
            {
                Npcs = nData
            };
            
            return townData;
        }

        public void OnLoadData(object data)
        {
            if (data == null)
            {
                LoadDefaultData();
            }
            
            var townData = (TownData) data;
            
            foreach (var npc in _npcs)
            {
                if (townData.Npcs.ContainsKey(npc.SaveKey))
                {
                    npc.OnLoadData(townData.Npcs[npc.SaveKey]);
                }
            }
        }

        public void LoadDefaultData()
        {
            throw new System.NotImplementedException();
        }
    }
    
    /// <summary>
    /// For saving town data
    /// </summary>
    [Serializable]
    public class TownData
    {
        public Dictionary<string, NpcSave> Npcs;
    }
    
    
    /// <summary>
    /// For saving npc data
    /// </summary>
    [Serializable]
    public class NpcSave
    {
  
    }
    
    /// <summary>
    /// Handles saving and loading of town data
    /// </summary>
    public interface ISaveTownDataHandler : ISaveGame
    {
    }
}