// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 10

using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Inventory.MerchantInventoryFrontend;
using _Script.NPC.NPCFrontend._Script.NPC.NPCFrontend;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Npc : MonoBehaviour
    {
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        public string npcName;
        
        [SerializeField] private DialogueModule dialogueModule;

        private NpcDialogueUI _dialogueUI;
        protected Dictionary<NpcHandlerType, INpcHandler> NpcHandlers;
        private const float DialogueDistance = 1.5f;

        protected void Awake()
        {
            _dialogueUI = NpcDialogueUI.Instance;
        }

        public void OnMouseDown()
        {
            var check = Physics2D.OverlapCircle(transform.position, DialogueDistance, LayerMask.GetMask("Player"));
            if (check == null) return;
            
            
            _dialogueUI.StartDialogue(dialogueModule.dialogueLines);
            _dialogueUI.OnDialogueEnd += OnDialogueEnd;
        }

        protected virtual void OnDialogueEnd()
        {
            Debug.Log("sub" + NpcHandlers);
            _dialogueUI.OnDialogueEnd -= OnDialogueEnd;
        }
    }

    public enum NpcHandlerType
    {
        Merchant,
        QuestGiver,
        Trainer
    }
    
    // Dialogue module
    [System.Serializable]
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

    // Quest module
    [System.Serializable]
    public class QuestModule
    {
        [LabelText("Quest Name")]
        public string questName;

        [LabelText("Quest Description")]
        [MultiLineProperty]
        public string questDescription;

        [LabelText("Is Quest Active?")]
        public bool isActive = false;

        public void ActivateQuest()
        {
            isActive = true;
            Debug.Log($"Quest '{questName}' activated!");
        }

        public void CompleteQuest()
        {
            isActive = false;
            Debug.Log($"Quest '{questName}' completed!");
        }
    }
}
