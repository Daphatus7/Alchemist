// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 40

using System.Collections.Generic;

namespace _Script.NPC.NpcBackend
{
    public class MerchantNpc : Npc
    {
        protected void Awake()
        {
            NpcHandlers = new Dictionary<NpcHandlerType, INpcHandler>
            {
                { NpcHandlerType.Merchant, GetComponent<MerchantUnit>() }
            };
        }
        
        protected override void OnDialogueEnd()
        {
            base.OnDialogueEnd();
            NpcHandlers[NpcHandlerType.Merchant].LoadNpcModule();
        }
    }
}