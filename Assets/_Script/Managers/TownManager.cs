// Author : Peiyu Wang @ Daphatus
// 20 02 2025 02 42

using System;
using System.Collections.Generic;
using _Script.NPC.NpcBackend;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Managers
{
    public class TownManager : Singleton<TownManager>, ISaveTownDataHandler
    {
        [Header("References to Npc that will not be deleted when the town is disabled")]
        [SerializeField] private List<NpcBase> _npcs;
        public string SaveKey => "TownManager";
        
        protected override void Awake()
        {
            base.Awake();
            //Register with the save system
            ServiceLocator.Instance.Register<ISaveTownDataHandler>(this);
        }
        
        protected void OnDestroy()
        {
            if(ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<ISaveTownDataHandler>();
        }

        public void OnEnable()
        {
            SaveLoadManager.Instance.LoadTownData();
            foreach (var npc in _npcs)
            {
                if(npc != null)
                {
                    npc.gameObject.SetActive(true);
                }
            }
        }
        
        public void OnDisable()
        {
            Debug.Log("--------------leaving town");
            if(SaveLoadManager.Instance)
            {
                SaveLoadManager.Instance.SaveTownData();
            }            
            
            foreach (var npc in _npcs)
            {
                if (npc != null)
                {
                    npc.gameObject.SetActive(false);
                }
            }
        }

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

                if (npc == null)
                {
                    Debug.LogError("Npc is null");
                    continue;
                }
                if(string.IsNullOrEmpty(npc.SaveKey))
                {
                    Debug.LogError("SaveKey is empty");
                    continue;
                }
                
                var npcSave = npc.OnSaveData();
                Debug.Log($"Saving data for {npc.SaveKey}");
                if (!nData.TryAdd(npc.SaveKey, npcSave))
                {
                    Debug.LogError($"Failed to save data for {npc.SaveKey}");
                }
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
                Debug.Log("No data found, loading default data");
                LoadDefaultData();
                return;
            }
            
            var townData = (TownData) data;
            if (townData == null)
            {
                throw new Exception("Data is not of type TownData");
            }
            foreach (var npc in _npcs)
            {
                if (npc == null)
                {
                    Debug.LogError("Npc is null");
                    continue;
                }
                if(string.IsNullOrEmpty(npc.SaveKey))
                {
                    Debug.LogError("SaveKey is empty");
                    continue;
                }
                if (townData.Npcs.TryGetValue(npc.SaveKey, out var npcSave))
                {
                    npc.OnLoadData(npcSave);
                }
                else
                {
                    npc.LoadDefaultData();
                }
            }
        }

        public void LoadDefaultData()
        {
            _npcs.ForEach(npc => npc.LoadDefaultData());
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