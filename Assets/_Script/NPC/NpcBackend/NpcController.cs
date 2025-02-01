// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 23

using System;
using System.Collections;
using _Script.Character;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using _Script.Utilities.StateMachine;
using Sirenix.OdinInspector;
using UnityEngine;
using IInteractable = _Script.Interactable.IInteractable;

namespace _Script.NPC.NpcBackend
{
    public class NpcController : MonStateMachine, IInteractable, INpcDialogueHandler, INpcModuleControlHandler
    {
        
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        
        [SerializeField] private NpcInfo npcInfo;
        public string NpcId => npcInfo.NpcName;
        
        [SerializeField] private float dialogueDistance = 5f;
        
        #region Private Fields
        protected PlayerCharacter CurrentPlayer { get; set; }
        
        private NpcModuleBase[] _npcModules;
        
        private ConversationInstance _conversationInstance;
        private Coroutine _distanceCheckCoroutine;
        private readonly WaitForSeconds _distanceCheckWait = new WaitForSeconds(0.3f);

        #endregion

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
                        
                        InteractEnd();
                        
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
            if (player == null)
            {
                Debug.LogWarning("NpcController.Interact: Player is null.");
                return;
            }
            StartConversation(player);
        }


        public virtual void TerminateConversation()
        {
            OnConversationTerminated();
        }

        public void CloseMainUI()
        {
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
        
        public void InteractEnd()
        {
            _conversationInstance?.TerminateInteraction();
        }

        public void OnHighlight() { }

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

        
        public void AddMoreUIHandler(IUIHandler handler)
        {
            _conversationInstance?.AddNpcUIHandler(handler);
        }

        public void RemoveUIHandler(IUIHandler handler)
        {
            _conversationInstance?.RemoveNpcUIHandler(handler);
        }



        public INpcModuleHandler[] GetAddonModules()
        {
            return GetComponents<INpcModuleHandler>();
        }

        public NpcInfo GetNpcDialogue()
        {
            return npcInfo;
        }
    }
    
    [Serializable]
    public class NpcInfo
    {
        [SerializeField] private string npcName; public string NpcName => npcName;
        [SerializeField] private string npcDialogue; public string NpcDialogue => npcDialogue;
    }
    
    public interface INpcDialogueHandler
    {
        INpcModuleHandler[] GetAddonModules();
        NpcInfo GetNpcDialogue();
        void TerminateConversation();
        void CloseMainUI();
    }
}