// Author : Peiyu Wang @ Daphatus
// 26 12 2024 12 21

using _Script.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map
{
    //Spawn point for player
    public class SpawnerPoint : Singleton<SpawnerPoint>
    {

        protected override void Awake()
        {
            base.Awake();
        }

        [Button]
        public Transform GetSpawnPoint()
        {              
            return gameObject.transform;
        }
    }
}