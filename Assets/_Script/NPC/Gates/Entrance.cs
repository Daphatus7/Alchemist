// Author : Peiyu Wang @ Daphatus
// 16 12 2024 12 07

using _Script.Map.WorldMap;
using _Script.NPC.NpcBackend;

namespace _Script.NPC.Gates
{
    public class Entrance : NpcController
    {
        protected override void OnDialogueEnd()
        {
            base.OnDialogueEnd();
            
            // need hard fix
            
            MapExplorerUI.Instance.MarkCurrentNodeAsExplored();
            MapExplorerUI.Instance.ShowUI();
            AddMoreUIHandler(MapExplorerUI.Instance);
        }
    }
}