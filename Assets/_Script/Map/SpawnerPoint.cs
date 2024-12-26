// Author : Peiyu Wang @ Daphatus
// 26 12 2024 12 21

using UnityEngine;

namespace _Script.Map
{
    public class SpawnerPoint : Singleton<SpawnerPoint>
    {
        private Transform _spawnPoint;

        protected override void Awake()
        {
            base.Awake();
            _spawnPoint = GetComponent<Transform>();
        }

        public Transform GetSpawnPoint()
        {
            return _spawnPoint;
        }
    }
}