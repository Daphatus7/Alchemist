// Npc.cs
using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Interactable;
using _Script.Managers;
using _Script.NPC.NPCFrontend._Script.NPC.NPCFrontend;
using _Script.UserInterface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    [RequireComponent(typeof(Collider2D))]
    public abstract class Npc : MonoBehaviour, IInteractable
    {
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        public string npcName;
        
        [SerializeField] private DialogueModule dialogueModule;

        protected Dictionary<NpcHandlerType, INpcHandler> NpcHandlers;
        private const float DialogueDistance = 1.5f;
        
        private InteractionInstance _conversationInstance;
        
        public void Interact(GameObject player)
        {
            // Prevent multiple interactions
            if (_conversationInstance != null) return;

            // Create a Conversation Interaction Instance
            _conversationInstance = new InteractionInstance(this, player.GetComponent<PlayerCharacter>());
            
            // Subscribe to the event so NPC can know when the conversation ends
            _conversationInstance.OnInteractionTerminated += OnConversationTerminated;

            StartInteraction();
        }

        public void InteractEnd()
        {
            if(_conversationInstance == null) return;
            // NPC actively ends the conversation
            _conversationInstance.TerminateInteraction();
            // The event OnInteractionTerminated will be invoked from inside TerminateInteraction()
        }

        public void AddMoreUIHandlers(IUIHandler handler)
        {
            if(_conversationInstance != null)
            {
                _conversationInstance.AddNpcUIHandler(handler);
            }
        }
        
        public void RemoveUIHandler(IUIHandler handler)
        {
            if(_conversationInstance != null)
            {
                _conversationInstance.RemoveNpcUIHandler(handler);
            }
        }

        public void OnHighlight() { }

        public void OnHighlightEnd() { }
        
        private void StartInteraction()
        {
            var check = Physics2D.OverlapCircle(transform.position, DialogueDistance, LayerMask.GetMask("Player"));
            if (check == null) return;
            
            NpcDialogueUI.Instance.StartDialogue(dialogueModule.dialogueLines);
            NpcDialogueUI.Instance.OnDialogueEnd += OnDialogueEnd;
        }
        
        protected virtual void OnDialogueEnd()
        {
            Debug.Log("Npc: Dialogue ended from UI side");
            NpcDialogueUI.Instance.OnDialogueEnd -= OnDialogueEnd;
            // Here you may want to let the ConversationInstance know the dialogue ended logically
            // But if you're just waiting for the player to close or manager to end, it's optional.
        }

        private void OnConversationTerminated()
        {
            // This method gets called when InteractionInstance is terminated from anywhere
            Debug.Log($"Npc: Conversation Terminated Notification Received");
            _conversationInstance.OnInteractionTerminated -= OnConversationTerminated;
            _conversationInstance = null;
        }
    }

    /// <summary>
    /// The player and the NPC are both holding the instance.
    /// Either side can terminate the interaction.
    /// </summary>
    public class InteractionInstance
    {
        private Npc _npc;
        private PlayerCharacter _player;
        private bool _isActive = false;
        
        private readonly List<IUIHandler> _npcUIHandlers = new List<IUIHandler>();

        // Event that notifies that the interaction has ended
        public event Action OnInteractionTerminated;

        public InteractionInstance(Npc npc, PlayerCharacter player)
        {
            _npc = npc;
            _player = player;
            ConversationManager.Instance.RegisterConversationInstance(this);
        }
        
        public void AddNpcUIHandler(IUIHandler handler)
        {
            _npcUIHandlers.Add(handler);
        }
        
        public void RemoveNpcUIHandler(IUIHandler handler)
        {
            _npcUIHandlers.Remove(handler);
        }
        
        public void TerminateInteraction()
        {
            CloseAllUI();
            // Trigger the event to let subscribers (NPC, Manager, etc.) know the interaction ended
            OnInteractionTerminated?.Invoke();

            // Clear references to allow GC
            _npc = null;
            _player = null;
            _npcUIHandlers.Clear();
        }
        
        private void CloseAllUI()
        {
            foreach (var handler in _npcUIHandlers)
            {
                handler.HideUI();
            }
        }
    }

    public enum NpcHandlerType
    {
        Merchant,
        QuestGiver,
        Trainer
    }
    
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

    [Serializable]
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
