// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 39

using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [Serializable]
    public class DialogueModule
    {
        [LabelText("Dialogue Lines")]
        [ListDrawerSettings(ShowIndexLabels = true)]
        public string[] dialogueLines;

        public void StartDialogue()
        {
            Debug.Log("Starting Dialogue...");
            foreach (var line in dialogueLines)
            {
                Debug.Log($"NPC says: {line}");
            }
        }
    }
}