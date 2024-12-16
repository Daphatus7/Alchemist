
// ConversationManager.cs
using _Script.NPC.NpcBackend;
using UnityEngine;

namespace _Script.Managers
{
    public interface IConversationManager
    {
        void RegisterConversationInstance(InteractionInstance instance);
        void EndConversation();
    }

    public class ConversationManager : Singleton<ConversationManager>, IConversationManager
    {
        
        private InteractionInstance _conversationInstance;
        
        public void RegisterConversationInstance(InteractionInstance instance)
        {
            if (_conversationInstance != null)
            {
                Debug.Log("ConversationManager: Conversation already started, closing it and starting a new one");
                _conversationInstance.TerminateInteraction();
            }
            _conversationInstance = instance;
            Debug.Log("ConversationManager: Conversation Started");
        }

        public void EndConversation()
        {
            if(_conversationInstance == null) return;
            Debug.Log("ConversationManager: Ending Conversation");
            _conversationInstance.TerminateInteraction();
            _conversationInstance = null;
        }
    }
}