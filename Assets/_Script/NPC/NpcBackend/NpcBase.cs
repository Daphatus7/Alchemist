// Author : Peiyu Wang @ Daphatus
// 08 02 2025 02 47

using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    
    /// <summary>
    /// Provides basic interaction as a Npc
    /// That is, when interact, pop up a dialogue
    /// </summary>
    public class NpcBase : MonoBehaviour, IInteractable, INpcDialogueHandler
    {
        private ConversationInstance _conversationInstance;
        private Coroutine _distanceCheckCoroutine;
        protected PlayerCharacter CurrentPlayer { get; set; }

        [SerializeField] private float dialogueDistance = 5f;
        /// <summary>
        /// When the player hit the mouse button
        /// </summary>
        /// <param name="player"></param>
        public void Interact(PlayerCharacter player)
        {
            if (player == null)
            {
                Debug.LogWarning("NpcController.Interact: Player is null.");
                return;
            }
            StartConversation(player);
        }
        
        /// <summary>
        /// Conversation starts here
        /// </summary>
        /// <param name="player"></param>
        private void StartConversation(PlayerCharacter player)
        {
            if (_conversationInstance != null)
            {
                Debug.Log("Conversation already in progress");
                return;
            }
            
            CurrentPlayer = player;
            
            _conversationInstance = new ConversationInstance();
            _conversationInstance.RegisterInteractionTerminatedEvent(OnConversationTerminated);
            
            
            // ui service
            var npcUIService = ServiceLocator.Instance.Get<INpcUIService>();
            if (npcUIService == null)
            {
                Debug.Log("NpcController.StartConversation: INpcUIService not found.");
                return;
            }

            // Delegate dialogue display to the UI service.
            npcUIService.StartDialogue(this);
            
            // If the UI service also implements IUIHandler, add it to the conversation instance.
            if (npcUIService is IUIHandler uiHandler)
            {
                _conversationInstance.AddNpcUIHandler(uiHandler);
            }
            else
            {
                Debug.LogWarning("NpcController.StartConversation: INpcUIService does not implement IUIHandler.");
            }

            // Start monitoring the player's distance.
            _distanceCheckCoroutine = StartCoroutine(CheckPlayerDistance());
        }

        public virtual void OnHighlight() { }

        public void OnHighlightEnd() { }
        
        /// <summary>
        /// Callback invoked when the conversation is terminated.
        /// </summary>
        private void OnConversationTerminated()
        {
            if (_conversationInstance != null)
            {
                _conversationInstance.UnregisterInteractionTerminatedEvent(OnConversationTerminated);
                _conversationInstance = null;
            }

            // Stop the distance-checking coroutine if it's running.
            if (_distanceCheckCoroutine != null)
            {
                StopCoroutine(_distanceCheckCoroutine);
                _distanceCheckCoroutine = null;
            }

            CurrentPlayer = null;
        }
        
        /// <summary>
        /// Coroutine to continuously check the player's distance from the NPC.
        /// If the player leaves the allowed range, end the conversation.
        /// </summary>
        private IEnumerator CheckPlayerDistance()
        {
            while (_conversationInstance != null)
            {
                if (CurrentPlayer)
                {
                    float distance = Vector3.Distance(transform.position, CurrentPlayer.transform.position);
                    // If player goes beyond DialogueDistance, end the conversation.
                    if (distance > dialogueDistance)
                    {
                        //end npc here
                        _conversationInstance?.TerminateInteraction();
                        
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.3f); 
            }
        }

        public INpcModuleHandler[] GetAddonModules()
        {
            throw new System.NotImplementedException();
        }

        public NpcInfo GetNpcDialogue()
        {
            throw new System.NotImplementedException();
        }

        public void TerminateConversation()
        {
            throw new System.NotImplementedException();
        }

        public void CloseMainUI()
        {
            throw new System.NotImplementedException();
        }
    }
}