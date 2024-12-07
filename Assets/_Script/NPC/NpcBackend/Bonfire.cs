// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 21

using System;
using _Script.Character;
using _Script.Places;
using UnityEngine;

namespace _Script.NPC.NpcBackend
{
    public class Bonfire : Npc
    {
        protected override void OnDialogueEnd()
        {
            base.OnDialogueEnd();
            Debug.Log("Bonfire is lit");
            PlaceManager.Instance.TeleportPlayerToTown(_player);
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