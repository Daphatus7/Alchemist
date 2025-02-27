// Author : Peiyu Wang @ Daphatus
// 08 02 2025 02 47

using System;
using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.Managers;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    
    /// <summary>
    /// Provides basic interaction as a Npc
    /// That is, when interacting, pop up a dialogue
    /// </summary>
    public abstract class NpcBase : MonoBehaviour, IInteractable, INpcSaveDataHandler
    {
        protected ConversationInstance ConversationInstance;
        private Coroutine _distanceCheckCoroutine;
        public string Name => gameObject.name;
        protected PlayerCharacter CurrentPlayer { get; set; }
        [SerializeField] private float dialogueDistance = 2f;
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
        
        public virtual void Awake()
        {
        }
        

        public virtual void OnHighlight() { }
        public virtual void OnHighlightEnd() { }
        public void InteractEnd()
        {
            
        }

        /// <summary>
        /// Conversation starts here
        /// </summary>
        /// <param name="player"></param>
        protected virtual void StartConversation(PlayerCharacter player)
        {
            if (ConversationInstance != null)
            {
                Debug.Log("Conversation already in progress");
                return;
            }
            
            CurrentPlayer = player;
            
            ConversationInstance = new ConversationInstance();
            ConversationInstance.RegisterInteractionTerminatedEvent(OnConversationTerminated);
            // Start monitoring the player's distance.
            _distanceCheckCoroutine = StartCoroutine(CheckPlayerDistance());
            
            /*** 这里添加需要加载的元件
             */
        }


        
        /// <summary>
        /// Callback invoked when the conversation is terminated.
        /// </summary>
        protected virtual void OnConversationTerminated()
        {
            if (ConversationInstance != null)
            {
                ConversationInstance.UnregisterInteractionTerminatedEvent(OnConversationTerminated);
                ConversationInstance = null;
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
            while (ConversationInstance != null)
            {
                if (CurrentPlayer)
                {
                    float distance = Vector3.Distance(transform.position, CurrentPlayer.transform.position);
                    // If player goes beyond DialogueDistance, end the conversation.
                    if (distance > dialogueDistance)
                    {
                        //end npc here
                        ConversationInstance?.TerminateInteraction();
                        
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.3f); 
            }
        }

        #region Save and Load
        
        public abstract string SaveKey { get; }
        public abstract NpcSave OnSaveData();
        public abstract void OnLoadData(NpcSave data);
        public abstract void LoadDefaultData();
        
        #endregion
    }
    
    public interface INpcSaveDataHandler
    {
        void OnLoadData(NpcSave npcBase);
        new NpcSave OnSaveData();
        new void LoadDefaultData();
        new string SaveKey { get; }
    }
}