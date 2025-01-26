// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 21

using _Script.Character;
using _Script.Managers;
using _Script.NPC.NpcBackend;
using _Script.Places;
using UnityEngine;

namespace _Script.NPC.Gates
{
    public class Bonfire : Npc
    {
        protected override void OnDialogueEnd()
        {
            base.OnDialogueEnd();
            Debug.Log("Bonfire is lit");
            
            GameManager.Instance.LoadMainScene("TownMap");
            PlaceManager.Instance.TeleportPlayerToTown(GameManager.Instance.GetPlayer());
            GameManager.Instance.ResetHexMap();
            //Mark the current dungeon as completed
            GameManager.Instance.UnloadCurrentAdditiveScene();
            //MapExplorerUI.Instance.MarkExploringNodeAsCompleted();
        }

        private PlayerCharacter _player;
        
        protected void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _player = other.GetComponent<PlayerCharacter>();
                _player.SetInSafeZone(true);
            }
        }
        
        protected void OnTriggerExit2D(Collider2D other)
        {
            if(other.CompareTag("Player"))
            {
                _player = other.GetComponent<PlayerCharacter>();
                _player.SetInSafeZone(false);
            }
        }
    }
}