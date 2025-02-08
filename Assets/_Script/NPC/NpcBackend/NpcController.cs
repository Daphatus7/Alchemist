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
    public class NpcController : NpcBase, INpcDialogueHandler, INpcModuleControlHandler
    {
        
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        [SerializeField] private NpcInfo npcInfo;
        public string NpcId => npcInfo.NpcName;
        
        private NpcModuleBase[] _npcModules;
        
        /// <summary>
        /// Conversation starts here
        /// </summary>
        /// <param name="player"></param>
        protected override void StartConversation(PlayerCharacter player)
        {
            base.StartConversation(player);
            
            // Modular
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
                ConversationInstance.AddNpcUIHandler(uiHandler);
            }
            else
            {
                Debug.LogWarning("NpcController.StartConversation: INpcUIService does not implement IUIHandler.");
            }

        }
        
        public void AddMoreUIHandler(IUIHandler handler)
        {
            ConversationInstance?.AddNpcUIHandler(handler);
        }

        public void RemoveUIHandler(IUIHandler handler)
        {
            ConversationInstance?.RemoveNpcUIHandler(handler);
        }
        
        public INpcModuleHandler[] GetAddonModules()
        {
            return GetComponents<INpcModuleHandler>();
        }

        public NpcInfo GetNpcDialogue()
        {
            return npcInfo;
        }

        public virtual void TerminateConversation()
        {
            OnConversationTerminated();
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
    }
}