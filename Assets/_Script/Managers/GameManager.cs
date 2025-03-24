using System.Collections.Generic;
using _Script.Character;
using _Script.Character.PlayerRank;
using _Script.Character.PlayerUI;
using _Script.Managers.GlobalUpdater;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.MapManager;
using _Script.Places;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        
        [SerializeField] private AstarPath _astarPath; public AstarPath AstarPath => _astarPath;

        private PlayerCharacter _playerCharacter; public PlayerCharacter PlayerCharacter => _playerCharacter;
        [SerializeField] GameObject _playerPrefab;
        [SerializeField] private StatsDisplay statsDisplay;
        [SerializeField] private Camera _mainCamera;
        public NiRank PlayerRank => _playerCharacter.Rank;

        [SerializeField] private string _startingScene = "TownMap";
        

        // The non-static class that manages scene loading
        private LevelManager _levelManager;
        
        private Scene _currentScene;

        // Any "global updaters" you have
        private List<IGlobalUpdate> _globalUpdaters = new List<IGlobalUpdate>();
        
        private void Start()
        {
            
            // Create and initialize the LevelManager
            _levelManager = new LevelManager();
            
            _levelManager.Initialize(_playerCharacter);
            
            //set scene as persistent

            //Map data
            //Should be loaded first
            // -------------- Load current scene instead
            LoadMainScene();
            MapManager.Instance.InitializeMaps();

            
            // Spawn the player character
            SpawnPlayer();
            
            //initialize data
            SaveLoadManager.Instance.LoadPlayerData();
            
            //initialize UI
            _playerCharacter.InitializeUI();
            
            //load save data
            statsDisplay.InitializeUI(_playerCharacter);
            
            //initialize MapManager
        }

        /// <summary>
        /// Called when the player is dead.
        /// 1. spawn the player in town
        /// 2. instanced data are not saved
        /// </summary>
        public void OnPlayerDeath()
        {
            //Save player persistent data
            //Do not save instanced data
            //load town map
            ResetPlayer();
            TeleportPlayerToTown();
            //spawn player
            //move player to town
            //Debug.Log($"Main scene '{sceneName}' loaded.");
            //Move player to the spawn point in the main scene
        }

        [Button]
        public void AddPlayerExperience500()
        {
            _playerCharacter.AddExperience(500);
        }
        [Button]
        public void AddPlayerGold500()
        {
            _playerCharacter.AddGold(500);
        }
        [Button]
        public void KillPlayer()
        {
            _playerCharacter.ApplyDamage(500f);
        }
        
        private void ResetPlayer()
        {
            _playerCharacter.ResetPlayer();
        }
        
        
        private void SpawnPlayer()
        {
            if (_playerPrefab != null)
            {
                var player = Instantiate(_playerPrefab, Vector3.zero, Quaternion.identity);
                _playerCharacter = player.GetComponent<PlayerCharacter>();
                ServiceLocator.Instance.Register<IPlayerSave>(_playerCharacter);
                if (_playerCharacter != null)
                {
                    //add essential components
                    DontDestroyOnLoad(_playerCharacter.gameObject);
                }
            }
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
        public void LoadSelectedScene(MapLoadContextInstance instance)
        {
            _levelManager.LoadSelectedScene(instance);
        }

        /// <summary> Unload the current additive scene. </summary>
        private void UnloadCurrentAdditiveScene()
        {
            _levelManager.UnloadCurrentAdditiveScene();
        }

        /// <summary> Load/replace the main scene (non-additive). </summary>
        private void LoadMainScene()
        {
            //problem is here, the scene is loaded again
            _levelManager.LoadMainScene(_startingScene);
        }
        
        public void TeleportPlayerToTown()
        {
            Instance.LoadMainScene();
            PlaceManager.Instance.TeleportPlayerToTown(GameManager.Instance.GetPlayer());
            //Mark the current dungeon as completed
            Instance.UnloadCurrentAdditiveScene();
        }
        #endregion
    }
}