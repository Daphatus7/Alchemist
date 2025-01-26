
// ConversationManager.cs
using _Script.NPC.NpcBackend;
using UnityEngine;

namespace _Script.Managers
{
    public interface IConversationManager
    {
        void RegisterConversationInstance(ConversationInstance instance);
        void EndConversation();
    }

    public class ConversationManager : Singleton<ConversationManager>, IConversationManager
    {
        
        private ConversationInstance _conversationInstance;
        
        public void RegisterConversationInstance(ConversationInstance instance)
        {
            if (_conversationInstance != null)
            {
                _conversationInstance.TerminateInteraction();
            }
            _conversationInstance = instance;
        }

        public void EndConversation()
        {
            if(_conversationInstance == null) return;
            _conversationInstance.TerminateInteraction();
            _conversationInstance = null;
        }
    }
}