// Author : Peiyu Wang @ Daphatus
// 05 12 2024 12 39

using _Script.Character;
using UnityEngine;

namespace _Script.Places
{
    public class PlaceManager : Singleton<PlaceManager>
    {
        [SerializeField] private Transform playerSpawnPoint; public Transform PlayerSpawnPoint => playerSpawnPoint;
        [SerializeField] private Transform townSpawnPoint; public Transform TownSpawnPoint => townSpawnPoint;
        
        public bool TeleportPlayerToTown(PlayerCharacter playerCharacter)
        {
            if (!TownSpawnPoint)
            {
                playerCharacter.transform.position = Vector3.zero;
            }
            else
            {
                playerCharacter.transform.position = TownSpawnPoint.position;
            }
            return true;
        }
    }
}