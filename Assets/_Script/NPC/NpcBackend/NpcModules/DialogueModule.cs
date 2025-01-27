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
        [LabelText("Dialogue Lines")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public override NpcHandlerType HandlerType => NpcHandlerType.Dialogue;
        public override string ModuleName => "Chat";
        
        public string[] dialogueLines;

        public void StartDialogue()
        {
            
        }

        public void LoadNpcModule()
        {
            throw new NotImplementedException();
        }

        public void UnloadNpcModule()
        {
            throw new NotImplementedException();
        }


    }
}