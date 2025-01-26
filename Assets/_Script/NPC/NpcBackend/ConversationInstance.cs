// Author : Peiyu Wang @ Daphatus
// 26 01 2025 01 58

using System;
using System.Collections.Generic;
using _Script.Character;
using _Script.Managers;
using _Script.UserInterface;

namespace _Script.NPC.NpcBackend
{
    /// <summary>
    /// The player and the NPC are both holding the instance.
    /// Either side can terminate the interaction.
    /// </summary>
    public class ConversationInstance
    {
        
        private readonly List<IUIHandler> _npcUIHandlers = new List<IUIHandler>();

        // Event that notifies that the interaction has ended
        public event Action OnInteractionTerminated;

        public ConversationInstance()
        {
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
}