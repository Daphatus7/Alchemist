// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 01

using System;
using UnityEngine;

namespace _Script.NPC.NpcBackend.NpcModules
{
    
    [DefaultExecutionOrder(500)]
    public abstract class NpcModuleBase : MonoBehaviour
    {
        public abstract NpcHandlerType HandlerType { get;}
        public abstract string ModuleName { get;}
        protected INpcModuleControlHandler Npc;

        protected virtual void Awake()
        {
            Npc = GetComponent<INpcModuleControlHandler>();
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