// Author : Peiyu Wang @ Daphatus
// 03 12 2024 12 10

using _Script.NPC.NPCFrontend._Script.NPC.NPCFrontend;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Npc : MonoBehaviour
    {
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        public string npcName;
        
        private BoxCollider2D _collider;
        
        [SerializeField] private DialogueModule dialogueModule;

        [SerializeField] private NpcDialogueUI dialogueUI;

        public void OnEnable()
        {
            dialogueUI.OnDialogueEnd += OnDialogueEnd;
        }

        public void OnMouseDown()
        {
            dialogueUI.StartDialogue(dialogueModule.dialogueLines);
        }
        
        public void OnDialogueEnd()
        {
            Debug.Log("Dialogue Ended!");
        }
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
