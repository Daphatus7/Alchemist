// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 30

using _Script.NPC.NpcBackend;
using _Script.Utilities.ServiceLocator;

namespace _Script.NPC.NPCFrontend
{
    /// <summary>
    /// For backend to call frontend UI
    /// </summary>
    public interface INpcUiCallback : INpcUIService
    {
        void LoadQuestUi(INpcQuestModuleHandler quest);
    }
}