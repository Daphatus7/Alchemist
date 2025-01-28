// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 01

using System;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    
    [DefaultExecutionOrder(500)]
    public abstract class NpcModuleBase : MonoBehaviour, INpcModuleHandler
    {
        public virtual void LoadNpcModule()
        {
            throw new NotImplementedException();
        }

        public virtual void UnloadNpcModule()
        {
            throw new NotImplementedException();
        }

        public NpcModuleInfo ModuleInfo { get; }
        public abstract NpcHandlerType HandlerType { get;}
        
        [SerializeField] private NpcModuleInfo moduleInfo;
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