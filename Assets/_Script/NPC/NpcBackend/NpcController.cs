// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 23

using System;
using _Script.Character;
using _Script.UserInterface;
using _Script.Utilities.ServiceLocator;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    public class NpcController : NpcControllerBase
    {
        [BoxGroup("Basic Info")]
        [LabelText("NPC Name"), Tooltip("Name of the NPC")]
        [SerializeField] private NpcInfo npcInfo;

        /// <summary>
        /// ID or name of this NPC
        /// </summary>
        public override string NpcId => npcInfo.NpcName;

        /// <summary>
        /// Called when the player interacts with this NPC
        /// </summary>
        public override void Interact(PlayerCharacter player)
        {
            if (player == null)
            {
                Debug.LogWarning("NpcController.Interact: Player is null.");
                return;
            }
            StartConversation(player);
        }

        /// <summary>
        /// Actually starts the conversation logic
        /// </summary>
        private void StartConversation(PlayerCharacter player)
        {
            if (_conversationInstance != null)
            {
                Debug.Log("Conversation already in progress.");
                return;
            }

            CurrentPlayer = player;

            _conversationInstance = new ConversationInstance();
            _conversationInstance.RegisterInteractionTerminatedEvent(OnConversationTerminated);

            var npcUIService = ServiceLocator.Instance.Get<INpcUIService>();
            if (npcUIService == null)
            {
                Debug.LogWarning("NpcController.StartConversation: INpcUIService not found.");
                return;
            }

            // Delegate dialogue display to the UI service
            npcUIService.StartDialogue(this);

            // If the UI service also implements IUIHandler, add it to the conversation instance
            if (npcUIService is IUIHandler uiHandler)
            {
                _conversationInstance.AddNpcUIHandler(uiHandler);
            }
            else
            {
                Debug.LogWarning("NpcController.StartConversation: INpcUIService does not implement IUIHandler.");
            }

            // Start monitoring the player's distance
            _distanceCheckCoroutine = StartCoroutine(CheckPlayerDistance());
        }

        /// <summary>
        /// Returns add-on modules implemented by this NPC
        /// </summary>
        public override INpcModuleHandler[] GetAddonModules()
        {
            return GetComponents<INpcModuleHandler>();
        }

        /// <summary>
        /// Returns this NPC's dialogue data
        /// </summary>
        public override NpcInfo GetNpcDialogue()
        {
            return npcInfo;
        }
    }

    /// <summary>
    /// Simple container for NPC info
    /// </summary>
    [Serializable]
    public class NpcInfo
    {
        [SerializeField] private string npcName;
        public string NpcName => npcName;

        [SerializeField] private string npcDialogue;
        public string NpcDialogue => npcDialogue;
    }
    
    public interface INpcDialogueHandler
    {
        INpcModuleHandler[] GetAddonModules();
        NpcInfo GetNpcDialogue();
        void TerminateConversation();
        void CloseMainUI();
    }
}