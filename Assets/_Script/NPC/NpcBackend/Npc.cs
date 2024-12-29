// Npc.cs
using System;
using System.Collections;
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
        private PlayerCharacter _currentPlayer;
        private Coroutine _distanceCheckCoroutine;
        
        public void Interact(GameObject player)
        {
            if (_conversationInstance != null) return;

            _currentPlayer = player.GetComponent<PlayerCharacter>();
            if (_currentPlayer == null) return;

            _conversationInstance = new InteractionInstance(this, _currentPlayer);
            _conversationInstance.OnInteractionTerminated += OnConversationTerminated;

            StartInteraction();

            // Register the dialogue UI as an IUIHandler so it can be closed automatically
            AddMoreUIHandlers(NpcDialogueUI.Instance);
            
            // Start a coroutine to monitor player distance
            _distanceCheckCoroutine = StartCoroutine(CheckPlayerDistance());
        }

        public void InteractEnd()
        {
            if(_conversationInstance == null) return;
            _conversationInstance.TerminateInteraction();
            // OnConversationTerminated will be called from within TerminateInteraction
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

            // Start Dialogue
            NpcDialogueUI.Instance.StartDialogue(dialogueModule.dialogueLines);
            NpcDialogueUI.Instance.OnDialogueEnd += OnDialogueEnd;
        }

        protected virtual void OnDialogueEnd()
        {
            NpcDialogueUI.Instance.OnDialogueEnd -= OnDialogueEnd;
            // Here we just stop listening, we don't end the conversation yet
            // Ending conversation can be triggered by player leaving or NPC calling InteractEnd()
        }

        private void OnConversationTerminated()
        {
            _conversationInstance.OnInteractionTerminated -= OnConversationTerminated;
            _conversationInstance = null;

            // Stop distance checking if it's still running
            if (_distanceCheckCoroutine != null)
            {
                StopCoroutine(_distanceCheckCoroutine);
                _distanceCheckCoroutine = null;
            }

            _currentPlayer = null;
        }

        /// <summary>
        /// Coroutine to continuously check the player's distance from the NPC.
        /// If the player leaves the allowed range, end the conversation.
        /// </summary>
        private IEnumerator CheckPlayerDistance()
        {
            while (_conversationInstance != null)
            {
                if (_currentPlayer != null)
                {
                    float distance = Vector3.Distance(transform.position, _currentPlayer.transform.position);
                    // If player goes beyond DialogueDistance, end the conversation.
                    if (distance > DialogueDistance)
                    {
                        InteractEnd();
                        yield break;
                    }
                }

                yield return new WaitForSeconds(0.3f); 
            }
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
