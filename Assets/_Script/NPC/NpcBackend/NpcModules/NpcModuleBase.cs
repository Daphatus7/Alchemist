// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 01

using System;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    
    [DefaultExecutionOrder(500)]
    public abstract class NpcModuleBase : MonoBehaviour, INpcModuleHandler
    {
        public abstract void LoadNpcModule(INpcModuleHandler handler);

        public abstract void UnloadNpcModule(INpcModuleHandler handler);

        public NpcModuleInfo ModuleInfo => _moduleInfo ??= new NpcModuleInfo(ModuleName, ModuleDescription, HandlerType);
        public virtual NpcHandlerType HandlerType => _moduleInfo.HandlerType;
        public abstract string ModuleDescription { get; }
        
        private NpcModuleInfo _moduleInfo;
        public abstract string ModuleName { get;}
        protected INpcModuleControlHandler Npc;

        protected virtual void Awake()
        {
            Npc = GetComponent<INpcModuleControlHandler>();
        }
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
        Dialogue
    }
}