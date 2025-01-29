// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 39

using System;
using _Script.NPC.NpcBackend.NpcModules;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [Serializable]
    public class DialogueModule : NpcModuleBase, INpcModuleHandler
    {
        public override void LoadNpcModule()
        {
            throw new NotImplementedException();
        }

        public override void UnloadNpcModule()
        {
            throw new NotImplementedException();
        }

        [LabelText("Dialogue Lines")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public override NpcHandlerType HandlerType => NpcHandlerType.Dialogue;

        public override string ModuleDescription => "Dialogue Module";
        public override string ModuleName => "Chat";
        
        public string[] dialogueLines;

        public void StartDialogue()
        {
            
        }
        
    }
}