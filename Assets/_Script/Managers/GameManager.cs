using System.Collections.Generic;
using _Script.Character;
using _Script.Character.PlayerRank;
using _Script.Managers.GlobalUpdater;
using _Script.Map;
using _Script.Map.WorldMap;
using _Script.Map.WorldMap.MapNode;
using _Script.Quest;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace _Script.Managers
{
    /// <summary>
    /// Persistent manager that lives throughout the entire game.
    /// Holds a LevelManager (factory for scene loading) and references like the PlayerCharacter.
    /// </summary>
    public class GameManager : PersistentSingleton<GameManager>
    {
        
        [SerializeField] private AstarPath _astarPath;

        [SerializeField] private PlayerCharacter _playerCharacter; public PlayerCharacter PlayerCharacter => _playerCharacter;
        
        public PlayerRankEnum PlayerRank => _playerCharacter.Rank;

        [SerializeField] private string _startingScene = "TownMap";

        // The non-static class that manages scene loading
        private LevelManager _levelManager;

        // Any "global updaters" you have
        private List<IGlobalUpdate> _globalUpdaters = new List<IGlobalUpdate>();

        private void Start()
        {
            // Make sure the player is also persistent
            if (_playerCharacter != null)
            {
                DontDestroyOnLoad(_playerCharacter.gameObject);
            }

            // Create and initialize the LevelManager
            _levelManager = new LevelManager();
            
            _levelManager.Initialize(_playerCharacter, _startingScene, _astarPath);

            // Optionally load the "starting" scene
            _levelManager.LoadMainScene(_startingScene);
        }
        
        private void UpdateGlobalUpdaters()
        {
            foreach (var updater in _globalUpdaters)
            {
                updater.Refresh();
            }
        }

        public void RegisterGlobalUpdater(IGlobalUpdate updater) => _globalUpdaters.Add(updater);
        public void UnregisterGlobalUpdater(IGlobalUpdate updater) => _globalUpdaters.Remove(updater);

        #region Public methods that forward calls to the LevelManager

        public PlayerCharacter GetPlayer() => _playerCharacter;

        /// <summary> Load a new additive scene based on NodeData. </summary>
        public void LoadSelectedScene(NodeDataInstance nodeDataInstance)
        {
            _levelManager.LoadSelectedScene(nodeDataInstance);
        }

        /// <summary> Unload the current additive scene. </summary>
        public void UnloadCurrentAdditiveScene()
        {
            _levelManager.UnloadCurrentAdditiveScene();
        }

        /// <summary> Move the player to a position in the target scene (if loaded). </summary>
        public void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {
            _levelManager.MovePlayerToScene(spawnPosition, targetScene);
        }

        /// <summary> Load/replace the main scene (non-additive). </summary>
        public void LoadMainScene(string sceneName)
        {
            _levelManager.LoadMainScene(sceneName);
        }

        /// <summary> Example method to reset your hex map. </summary>
        public void ResetHexMap()
        {
            MapController.Instance.ResetGrid();
        }

        #endregion
    }
}