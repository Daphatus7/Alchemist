// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 23

using System.Collections;
using _Script.Character;
using _Script.UserInterface;
using Sirenix.OdinInspector;
using UnityEngine;
using IInteractable = _Script.Interactable.IInteractable;

namespace _Script.NPC.NpcBackend
{
    /// <summary>
    /// Base class for NPC Controllers that encapsulates 
    /// conversation start/termination logic and checks.
    /// </summary>
    public abstract class NpcControllerBase : MonoBehaviour,
        IInteractable, INpcDialogueHandler, INpcModuleControlHandler
    {
        [BoxGroup("Basic Info")]
        [LabelText("Max Dialogue Distance"), Tooltip("Max distance for dialogue")]
        [SerializeField] protected float dialogueDistance = 5f;

        /// <summary>
        /// Current player in conversation with this NPC
        /// </summary>
        protected PlayerCharacter CurrentPlayer { get; set; }

        /// <summary>
        /// Holds info about an active conversation
        /// </summary>
        protected ConversationInstance _conversationInstance;

        /// <summary>
        /// Coroutine responsible for continuously checking the player's distance
        /// </summary>
        protected Coroutine _distanceCheckCoroutine;

        /// <summary>
        /// Unique ID for this NPC (e.g. name or identifier)
        /// </summary>
        public abstract string NpcId { get; }

        /// <summary>
        /// Called when the player interacts with this NPC
        /// </summary>
        public abstract void Interact(PlayerCharacter player);

        /// <summary>
        /// Terminates the conversation from external calls
        /// </summary>
        public virtual void TerminateConversation()
        {
            OnConversationTerminated();
        }

        /// <summary>
        /// Closes main UI from external calls (if needed)
        /// </summary>
        public virtual void CloseMainUI()
        {
            // Override in child if needed
        }

        /// <summary>
        /// Checks whether the player is in range and ends the conversation otherwise
        /// </summary>
        protected virtual IEnumerator CheckPlayerDistance()
        {
            while (_conversationInstance != null)
            {
                if (CurrentPlayer)
                {
                    float distance = Vector3.Distance(transform.position, CurrentPlayer.transform.position);
                    // If player goes beyond dialogueDistance, end the conversation.
                    if (distance > dialogueDistance)
                    {
                        _conversationInstance?.TerminateInteraction();
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.3f);
            }
        }

        /// <summary>
        /// Callback invoked when the conversation is terminated
        /// </summary>
        protected virtual void OnConversationTerminated()
        {
            if (_conversationInstance != null)
            {
                _conversationInstance.UnregisterInteractionTerminatedEvent(OnConversationTerminated);
                _conversationInstance = null;
            }

            // Stop the distance-checking coroutine if it's running
            if (_distanceCheckCoroutine != null)
            {
                StopCoroutine(_distanceCheckCoroutine);
                _distanceCheckCoroutine = null;
            }

            CurrentPlayer = null;
        }

        /// <summary>
        /// Returns add-on modules implemented by this NPC
        /// </summary>
        public abstract INpcModuleHandler[] GetAddonModules();

        /// <summary>
        /// Returns the NPC dialogue data
        /// </summary>
        public abstract NpcInfo GetNpcDialogue();

        /// <summary>
        /// Optionally add more UI handlers to the conversation
        /// </summary>
        public virtual void AddMoreUIHandler(IUIHandler handler)
        {
            _conversationInstance?.AddNpcUIHandler(handler);
        }

        /// <summary>
        /// Optionally remove UI handlers from the conversation
        /// </summary>
        public virtual void RemoveUIHandler(IUIHandler handler)
        {
            _conversationInstance?.RemoveNpcUIHandler(handler);
        }

        public virtual void OnHighlight() { }
        public virtual void OnHighlightEnd() { }
    }
}