// Author : Peiyu Wang @ Daphatus
// 24 03 2025 03 28

using System;
using System.Collections.Generic;
using _Script.Map.MapLoadContext.RewardContext;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _Script.Map.MapExit
{
    public class GateGroup : Singleton<GateGroup>
    {
        [SerializeField] private GameObject equipmentGate;
        [SerializeField] private GameObject bossGate;
        [SerializeField] private GameObject supplyGate;

        // spawn gate locations
        [SerializeField] private Transform[] spawnPoints; //start and end point
        private readonly Dictionary<RewardType, GameObject> _gates = new();

        protected override void Awake()
        {
            base.Awake();
            _gates.Add(RewardType.Equipment, equipmentGate);
            _gates.Add(RewardType.Boss, bossGate);
            _gates.Add(RewardType.Supply, supplyGate);
        }
        
        public void GenerateGates()
        {
            var nextLevelMaps = MapManager.MapManager.Instance.NextPossibleMaps;
            var gateCount = nextLevelMaps.Length;
            var locations = GetGateLocations(gateCount);
            for (var i = 0; i < gateCount; i++)
            {
                var gate = SpawnGate(_gates[nextLevelMaps[i].RewardType], locations[i]);
                gate.Initialize(nextLevelMaps[i].RewardType, nextLevelMaps[i]);
            }
        }
        
        private Gate SpawnGate(GameObject gate, Vector3 location)
        {
            var newGate = Instantiate(gate, transform);
            newGate.transform.position = location;
            if (newGate.TryGetComponent(out Gate gateComponent))
            {
                return gateComponent;
            }
            throw new Exception("Gate prefab does not have Gate component");
        }
        
        private Vector3[] GetGateLocations(int gateCount)
        {
            var locations = new Vector3[gateCount];
            if (gateCount == 1)
            {
                locations[0] = Vector3.Lerp(spawnPoints[0].position, spawnPoints[1].position, 0.5f);
            }
            else if (gateCount == 2)
            {
                locations[0] = Vector3.Lerp(spawnPoints[0].position, spawnPoints[1].position, 0.25f);
                locations[1] = Vector3.Lerp(spawnPoints[0].position, spawnPoints[1].position, 0.75f);
            }
            else if (gateCount == 3)
            {
                locations[0] = spawnPoints[0].position;
                locations[1] = Vector3.Lerp(spawnPoints[0].position, spawnPoints[1].position, 0.5f);
                locations[2] = spawnPoints[1].position;
            }
            return locations;
        }
    }
}