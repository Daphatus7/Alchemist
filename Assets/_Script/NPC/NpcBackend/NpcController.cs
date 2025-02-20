// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 23

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Character;
using _Script.Managers;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.UserInterface;
using _Script.Utilities.SaveGame;
using _Script.Utilities.ServiceLocator;
using _Script.Utilities.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;
using IInteractable = _Script.Interactable.IInteractable;

namespace _Script.NPC.NpcBackend
{
    public class NpcController : NpcBase, INpcDialogueHandler, INpcModuleControlHandler
    {
        
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        [SerializeField] private NpcInfo npcInfo;
        public string NpcId => npcInfo.NpcName;
        
        private NpcModuleBase[] _npcModules;
        
        /// <summary>
        /// Conversation starts here
        /// </summary>
        /// <param name="player"></param>
        protected override void StartConversation(PlayerCharacter player)
        {
            base.StartConversation(player);
            
            // Modular
            var npcUIService = ServiceLocator.Instance.Get<INpcUIService>();
            if (npcUIService == null)
            {
                Debug.Log("NpcController.StartConversation: INpcUIService not found.");
                return;
            }

            // Delegate dialogue display to the UI service.
            npcUIService.StartDialogue(this);
            
            // If the UI service also implements IUIHandler, add it to the conversation instance.
            if (npcUIService is IUIHandler uiHandler)
            {
                ConversationInstance.AddNpcUIHandler(uiHandler);
            }
            else
            {
                Debug.LogWarning("NpcController.StartConversation: INpcUIService does not implement IUIHandler.");
            }

        }
        
        public void AddMoreUIHandler(IUIHandler handler)
        {
            ConversationInstance?.AddNpcUIHandler(handler);
        }

        public void RemoveUIHandler(IUIHandler handler)
        {
            ConversationInstance?.RemoveNpcUIHandler(handler);
        }
        
        public INpcModuleHandler[] GetAddonModules()
        {
            return GetComponents<INpcModuleHandler>();
        }

        public NpcInfo GetNpcDialogue()
        {
            return npcInfo;
        }

        public virtual void TerminateConversation()
        {
            OnConversationTerminated();
        }


        #region Save and Load

        public override void LoadDefaultData()
        {
            throw new NotImplementedException();
        }

        public override string SaveKey => NpcId;

        /// <summary>
        /// Called by external script to save data.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override NpcSave OnSaveData()
        {
            var moduleSaveInstances = new Dictionary<string, NpcSaveModule>();
            
            //Pack all module save data
            foreach (var t in _npcModules)
            {
                if (t != null)
                {
                    if (t.OnSaveData() != null)
                    {
                        if (moduleSaveInstances.ContainsKey(t.ModuleInfo.ModuleName))
                        {
                            throw new Exception("NpcController.OnSaveData: Duplicate module name " + 
                                                t.ModuleInfo.ModuleName);
                        }
                        moduleSaveInstances.Add(t.ModuleInfo.ModuleName, t.OnSaveData());
                    }
                    else
                    {
                        throw new Exception("NpcController.OnSaveData: Module " + 
                                            t.ModuleName + " returned null save data.");
                    }
                }
            }
            
            //Pack all data
            var saveInstance = new NpcControllerSaveInstance
            {
                ModuleSaveInstances = moduleSaveInstances
            };
            return saveInstance;
        }

        public override void OnLoadData(NpcSave data)
        {
            if (data == null)
            {
                LoadDefaultData();
                return;
            }

            //Check data type
            if (data is not NpcControllerSaveInstance saveInstance)
            {
                throw new Exception("NpcController.OnLoadData: Invalid save data type.");
            }
            
            //Load all module data
            foreach (var moduleSaveInstance 
                     in saveInstance.ModuleSaveInstances)
            {
                var module = Array.Find(_npcModules, 
                    x => x.ModuleInfo.ModuleName == moduleSaveInstance.Key);
                if (module != null)
                {
                    module.OnLoadData(moduleSaveInstance.Value);
                }
                else
                {
                    Debug.LogWarning("NpcController.OnLoadData: Module " 
                                     + moduleSaveInstance.Key + " not found.");
                }
            }
        }


        #endregion
    }
    
    [Serializable]
    public class NpcInfo
    {
        [SerializeField] 
        private string npcName; public string NpcName => npcName;
        [SerializeField] 
        private string npcDialogue; public string NpcDialogue => npcDialogue;
    }
    [Serializable]
    public class NpcControllerSaveInstance : NpcSave
    {
        public Dictionary<string, NpcSaveModule> ModuleSaveInstances;
    }
    
    /// <summary>
    /// For each module
    /// </summary>
    [Serializable]
    public abstract class NpcSaveModule
    {
        
    }
    
    public interface INpcDialogueHandler
    {
        INpcModuleHandler[] GetAddonModules();
        NpcInfo GetNpcDialogue();
        void TerminateConversation();
    }
}