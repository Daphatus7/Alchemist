// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 21

using _Script.Character;
using _Script.Managers;
using _Script.NPC.NpcBackend;
using UnityEngine;

namespace _Script.NPC.Gates
{
    public class Bonfire : NpcController
    {
        public override void TerminateConversation()
        {
            base.TerminateConversation();
            GameManager.Instance.TeleportPlayerToTown();
            //MapExplorerUI.Instance.MarkExploringNodeAsCompleted();
        }
        
        protected void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                CurrentPlayer = other.GetComponent<PlayerCharacter>();
                CurrentPlayer.SetInSafeZone(true);
            }
        }
        
        protected void OnTriggerExit2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                CurrentPlayer = other.GetComponent<PlayerCharacter>();
                CurrentPlayer.SetInSafeZone(false);
                CurrentPlayer = null;
            }
        }
    }
}