using System.Collections.Generic;
using _Script.Character;
using _Script.Managers.GlobalUpdater;
using _Script.Map.WorldMap;
using _Script.Map.WorldMap.MapNode;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace _Script.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        private ServiceLocator _serviceLocator;

        [SerializeField] private PlayerCharacter _playerCharacter;
        [SerializeField] private string startingScene = "TownMap";

        private List<IGlobalUpdate> _globalUpdaters = new List<IGlobalUpdate>();

        // Reference to the LevelManager
        [SerializeField] private LevelManager _levelManager;
        
        private void Start()
        {
            // Initialize Service Locator
            _serviceLocator = ServiceLocator.Instance;

            // Ensure persistent objects like the player and manager
            MakePersistentObjects();
            
            // Initialize LevelManager and pass references it needs (if not directly assigned via inspector)
            _levelManager.Initialize(_playerCharacter, startingScene);

            // Optionally load the starting scene or handle any other initial logic
            _levelManager.LoadMainScene(startingScene);
        }

        public PlayerCharacter GetPlayer()
        {
            return _playerCharacter;
        }

        private void MakePersistentObjects()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_playerCharacter.gameObject);
        }

        // Global updaters
        private void UpdateGlobalUpdaters()
        {
            Debug.Log("Updating global updaters - Consider removing this log in production");
            foreach (var updater in _globalUpdaters)
            {
                updater.Refresh();
            }
        }

        public void RegisterGlobalUpdater(IGlobalUpdate updater)
        {
            _globalUpdaters.Add(updater);
        }

        public void UnregisterGlobalUpdater(IGlobalUpdate updater)
        {
            _globalUpdaters.Remove(updater);
        }

        // Methods to interface with the LevelManager
        public void LoadSelectedScene(NodeData nodeData)
        {
            _levelManager.LoadSelectedScene(nodeData);
        }

        public void UnloadCurrentAdditiveScene()
        {
            _levelManager.UnloadCurrentAdditiveScene();
            UpdateGlobalUpdaters();
        }

        public void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {
            _levelManager.MovePlayerToScene(spawnPosition, targetScene);
        }

        public void LoadMainScene(string townMap)
        {
            _levelManager.LoadMainScene(townMap);
        }

        public void ResetHexMap()
        {
            MapExplorerUI.Instance.ResetHexMap();
        }
    }
}