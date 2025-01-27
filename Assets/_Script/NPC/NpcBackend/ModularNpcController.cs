// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 23

using System;
using System.Collections;
using System.Collections.Generic;
using _Script.Character;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.NPC.NPCFrontend._Script.NPC.NPCFrontend;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using _Script.Utilities.StateMachine;
using Edgar.Unity.Examples;
using Sirenix.OdinInspector;
using UnityEngine;
using IInteractable = _Script.Interactable.IInteractable;

namespace _Script.NPC.NpcBackend
{
    public class ModularNpcController : MonStateMachine, IInteractable, INpcDialogueHandler, INpcModuleControlHandler
    {
        
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        
        [SerializeField] private NpcInfo npcInfo;
        
        [SerializeField] private float dialogueDistance = 5f;
        
        protected PlayerCharacter CurrentPlayer;
        
        private NpcModuleBase[] _npcModules;
        
        private ConversationInstance _conversationInstance;
        private Coroutine _distanceCheckCoroutine;
        
        protected override IState[] InitializeStateMachine()
        {
            var npcStates = GetAllNpcStates();
            var iStates = new IState[npcStates.Length];
            foreach (var npcState in npcStates)
            {
                npcState.Initialize();
            }
            for (int i = 0; i < npcStates.Length; i++)
            {
                iStates[i] = npcStates[i];
            }
            return iStates;
        }


        private NpcState.NpcState [] GetAllNpcStates()
        {
            return GetComponents<NpcState.NpcState>();
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
                        
                        
                        yield break;
                    }
                }
                yield return new WaitForSeconds(0.3f); 
            }
        }
        /// <summary>
        /// When the player hit the mouse button
        /// </summary>
        /// <param name="player"></param>
        public void Interact(PlayerCharacter player)
        {
            StartConversation(player);
        }


        protected virtual void OnDialogueEnd()
        {
            
        }
        
        private void StartConversation(PlayerCharacter player)
        {
            if (_conversationInstance != null) return;
            CurrentPlayer = player;
            
            _conversationInstance = new ConversationInstance();
            _conversationInstance.OnInteractionTerminated += OnConversationTerminated;
            
            
            //Delegate to the NPC UI Service
            ServiceLocator.Instance.Get<INpcUIService>().StartDialogue(this); //this displays all the possible options provided by the NPC
            
            // Start a coroutine to monitor player distance
            _distanceCheckCoroutine = StartCoroutine(CheckPlayerDistance());
        }
        
        public void InteractEnd()
        {
            if(_conversationInstance == null) return;
            _conversationInstance.TerminateInteraction();
            // OnConversationTerminated will be called from within TerminateInteraction
        }

        public void OnHighlight() { }

        public void OnHighlightEnd() { }

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

            CurrentPlayer = null;
        }

        
        public void AddMoreUIHandler(IUIHandler handler)
        {
            _conversationInstance?.AddNpcUIHandler(handler);
        }

        public void RemoveUIHandler(IUIHandler handler)
        {
            _conversationInstance?.RemoveNpcUIHandler(handler);
        }
        
        public INpcDialogueHandler[] GetAddonModules()
        {
            return GetComponents<INpcDialogueHandler>();
        }
        
    }
    
    [Serializable]
    public class NpcInfo
    {
        [SerializeField] private string npcName;
        [SerializeField] private string [] npcDialogue;
    }
    
    public interface INpcDialogueHandler
    {
        
        
        INpcDialogueHandler[] GetAddonModules();
    }

    public interface INpcDialogueModuleHandler
    {
         string OptionName();
         void LoadNpcModule();
    }
}