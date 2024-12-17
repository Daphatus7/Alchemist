// Author : Peiyu Wang @ Daphatus
// 16 12 2024 12 07

using _Script.Map.WorldMap;

namespace _Script.NPC.NpcBackend.Gates
{
    public class Entrance : Npc
    {
        protected override void OnDialogueEnd()
        {
            base.OnDialogueEnd();
            MapExplorerUI.Instance.ShowGrid();
        }
    }
}