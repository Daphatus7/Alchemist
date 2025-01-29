// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 30

using _Script.Quest;
using _Script.Utilities.ServiceLocator;

namespace _Script.NPC.NPCFrontend
{
    public interface INpcUiCallback : INpcUIService
    {
        void OnNpcUiChange(NpcUiType uiType);
        void LoadQuestUi(QuestDefinition quest);
    }
}