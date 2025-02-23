// Author : Peiyu Wang @ Daphatus
// 16 12 2024 12 07

using _Script.Map;
using _Script.Map.WorldMap;
using _Script.NPC.NpcBackend;

namespace _Script.NPC.Gates
{
    public class Entrance : NpcController
    {
        public override void TerminateConversation()
        {
            base.TerminateConversation();
            
            // need hard fix
            MapController.Instance.MarkCurrentNodeAsExplored();
            MapExplorerView.Instance.ShowUI();
            AddMoreUIHandler(MapExplorerView.Instance);
        }
    }
}