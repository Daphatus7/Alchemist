using System.Collections;
using System.Collections.Generic;
using _Script.Character;
using _Script.Managers.GlobalUpdater;
using _Script.Map;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace _Script.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        // Service Locator
        private ServiceLocator _serviceLocator;

        [SerializeField] private PlayerCharacter _playerCharacter; public PlayerCharacter GetPlayer()
        {
            return _playerCharacter;
        }
        [SerializeField] private string startingScene = "PrototypeScene";
        [SerializeField] private List<string> dungeonLevels;
        
        private List<IGlobalUpdate> _globalUpdaters = new List<IGlobalUpdate>();
        
        
        private void UpdateGlobalUpdaters()
        {
            Debug.Log("Updating global updaters + placed in a improper lines of code, remove it later");
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

        // Track the main (or primary) currently loaded scene
        private string currentMainScene;
        private string currentAdditiveScene;

        // Keep track of all currently loaded additive scenes
        private List<string> loadedAdditiveScenes = new List<string>();

        private void Start()
        {
            // Initialize Service Locator
            _serviceLocator = ServiceLocator.Instance;

            // Ensure persistent objects like the player and manager
            MakePersistentObjects();
        }


        /// <summary>
        /// Loads a new scene as the main scene (non-additive). Unloads any previously loaded main scene and all additive scenes.
        /// </summary>
        public void LoadMainScene(string sceneName)
        {
            if (sceneName == currentMainScene) return;
            StartCoroutine(LoadMainSceneAsync(sceneName));
        }

        /// <summary>
        /// Loads a scene additively (in addition to the current main scene and other additive scenes).
        /// </summary>
        public void LoadSelectedScene(MapNode sceneData)
        {
            // Avoid adding a scene that's already loaded
            if (loadedAdditiveScenes.Contains(sceneData.MapName))
            {
                Debug.LogWarning($"Scene {sceneData.MapName} is already loaded.");
                return;
            }

            currentAdditiveScene = sceneData.MapName;
            StartCoroutine(AddSceneAsync(sceneData.MapName));
        }
        
        public void UnloadCurrentAdditiveScene()
        {
            if(currentAdditiveScene != null)
                UnloadAdditiveScene(currentAdditiveScene);
            else Debug.LogWarning("No current additive scene to unload");
            UpdateGlobalUpdaters();
        }
        
        
        /// <summary>
        /// Unloads a currently loaded additive scene.
        /// </summary>
        private void UnloadAdditiveScene(string sceneName)
        {
            if (loadedAdditiveScenes.Contains(sceneName))
            {
                currentAdditiveScene = null;
                StartCoroutine(UnloadSceneAsync(sceneName));
            }
        }

        /// <summary>
        /// Moves the player to a position in a specific target scene. Ensures the target scene is loaded additively.
        /// </summary>
        public void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {
            if (loadedAdditiveScenes.Contains(targetScene))
            {
                _playerCharacter.transform.position = spawnPosition;
                Debug.Log($"Player moved to {spawnPosition} in scene {targetScene}.");
            }
            else
            {
                Debug.LogWarning($"Scene {targetScene} is not loaded. Player cannot be moved.");
            }
        }
        
        /// <summary>
        /// Ensure persistent objects like the player are not destroyed across scenes.
        /// </summary>
        private void MakePersistentObjects()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_playerCharacter.gameObject);
        }

        #region Private Coroutines and Helpers

        private IEnumerator LoadMainSceneAsync(string sceneName)
        {
            // Unload the current main scene if there is one
            if (!string.IsNullOrEmpty(currentMainScene))
            {
                yield return SceneManager.UnloadSceneAsync(currentMainScene);
                currentMainScene = null;
            }

            // Unload all additive scenes before loading a new main scene
            UnloadAllAdditiveScenes();
            loadedAdditiveScenes.Clear();

            // Using a coroutine to simulate a load; uncomment the lines below if you need actual async load
            // AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            // while (!asyncLoad.isDone)
            // {
            //     Debug.Log($"Loading main scene {sceneName}: {asyncLoad.progress * 100}%");
            //     yield return null;
            // }

            currentMainScene = sceneName;
            Debug.Log($"Main scene {sceneName} has been loaded.");

            // Optionally load a random dungeon scene additively after main scene load
            if (dungeonLevels != null && dungeonLevels.Count > 0)
            {
                var randomIndex = Random.Range(0, dungeonLevels.Count);
                string randomDungeonScene = dungeonLevels[randomIndex];
                Debug.Log($"Loading random dungeon scene: {randomDungeonScene}");
                StartCoroutine(AddSceneAsync(randomDungeonScene));
            }
        }
        
        private IEnumerator AddSceneAsync(string sceneName)
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Loading additive scene {sceneName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            // Ensure the scene is active if required
            Scene loadedScene = SceneManager.GetSceneByName(sceneName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
            }

            loadedAdditiveScenes.Add(sceneName);
            Debug.Log($"Additive scene {sceneName} has been loaded.");
            
            //The scene is loaded but not the map, now generate the map
            //wait
            OnMapLoaded(sceneName);
        }

        [SerializeField] private GameObject _spawnBonfirePrefab;
        private void OnMapLoaded(string newScene)
        {
            if (SubGameManager.Instance.GenerateMap(out Vector2Int spawnPoint, out Vector2Int endPoint))
            {
                MovePlayerToScene(new Vector3(spawnPoint.x, spawnPoint.y, 0), newScene);
                
                //Spawn bonfire at the spawn point
                Instantiate(_spawnBonfirePrefab, new Vector3(endPoint.x, endPoint.y, 0), Quaternion.identity);
            }
        }

        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                // Set a fallback scene if the scene being unloaded is the active scene
                if (SceneManager.GetActiveScene().name == sceneName && !string.IsNullOrEmpty(currentMainScene))
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentMainScene));
                }

                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(sceneName);
                while (asyncUnload != null && !asyncUnload.isDone)
                {
                    Debug.Log($"Unloading scene {sceneName}: {asyncUnload.progress * 100}%");
                    yield return null;
                }

                loadedAdditiveScenes.Remove(sceneName);
                Debug.Log($"Scene {sceneName} has been unloaded.");
            }
        }

        private IEnumerator ReloadMainSceneAsync(string sceneName)
        {
            yield return SceneManager.UnloadSceneAsync(sceneName);
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!asyncLoad.isDone)
            {
                Debug.Log($"Reloading {sceneName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            currentMainScene = sceneName;
            Debug.Log($"{sceneName} has been reloaded.");
        }
        
        /// <summary>
        /// Unloads all currently loaded additive scenes.
        /// </summary>
        private void UnloadAllAdditiveScenes()
        {
            foreach (var scene in loadedAdditiveScenes)
            {
                StartCoroutine(UnloadSceneAsync(scene));
            }
        }
        #endregion
    }
}
