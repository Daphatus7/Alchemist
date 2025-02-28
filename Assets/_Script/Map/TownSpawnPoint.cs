// Author : Peiyu Wang @ Daphatus
// 28 02 2025 02 22

using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map
{
    public class TownSpawnPoint : Singleton<TownSpawnPoint>
    {
        [Button]
        public Transform GetSpawnPoint()
        {              
            return gameObject.transform;
        }
    }
}