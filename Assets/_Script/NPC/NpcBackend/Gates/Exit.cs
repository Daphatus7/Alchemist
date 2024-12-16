// Author : Peiyu Wang @ Daphatus
// 16 12 2024 12 49

namespace _Script.NPC.NpcBackend.Gates
{
    public class Exit : Npc
    {
        protected override void OnDialogueEnd()
        {
            base.OnDialogueEnd();
            //Show the map explorer UI
        }
    }
}