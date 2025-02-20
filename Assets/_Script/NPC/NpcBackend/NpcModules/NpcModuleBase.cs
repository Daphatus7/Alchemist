// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 01

using System;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    
    [DefaultExecutionOrder(500)]
    public abstract class NpcModuleBase : MonoBehaviour, INpcModuleHandler, INpcSaveModule
    {
        public abstract bool ShouldLoadModule();
        public abstract void LoadNpcModule();
        public abstract void UnloadNpcModule();

        public string NpcId => Npc.NpcId;
        private NpcModuleInfo _moduleInfo;

        public NpcModuleInfo ModuleInfo => _moduleInfo ??= new NpcModuleInfo(ModuleName, ModuleDescription, HandlerType);
        public abstract NpcHandlerType HandlerType { get; }
        public abstract string ModuleDescription { get; }
        
        public abstract string ModuleName { get;}
        protected INpcModuleControlHandler Npc;
        
        protected virtual void Awake()
        {
            Npc = GetComponent<INpcModuleControlHandler>();
        }

        public abstract void OnLoadData(NpcSaveModule data);
        
        public abstract NpcSaveModule OnSaveData();
        public abstract void LoadDefaultData();
    }
    
    [Serializable]
    public class NpcModuleInfo
    {
        [SerializeField] private string moduleName; public string ModuleName => moduleName;
        [SerializeField] private string moduleDescription; public string ModuleDescription => moduleDescription;
        [SerializeField] private NpcHandlerType handlerType; public NpcHandlerType HandlerType => handlerType;
        
        public NpcModuleInfo(string moduleName, string moduleDescription, NpcHandlerType handlerType)
        {
            this.moduleName = moduleName;
            this.moduleDescription = moduleDescription;
            this.handlerType = handlerType;
        }
    }
    
    [Serializable]
    public enum NpcHandlerType
    {
        Merchant,
        QuestGiver,
        Trainer,
        Dialogue,
        GuildQuestGiver
    }
    
    public interface INpcSaveModule
    {
        string ModuleName { get; }
        void OnLoadData(NpcSaveModule data);
        NpcSaveModule OnSaveData();
        void LoadDefaultData();
    }
}