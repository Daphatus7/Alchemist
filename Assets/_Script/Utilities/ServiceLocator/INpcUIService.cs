// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 11

using _Script.NPC.NpcBackend;

namespace _Script.Utilities.ServiceLocator
{
    public interface INpcUIService : IUIService
    {
        void StartDialogue(INpcDialogueHandler dialogueHandler);
    }
}