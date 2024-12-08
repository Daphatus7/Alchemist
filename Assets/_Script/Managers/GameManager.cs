using System.Collections;
using System.Collections.Generic;
using _Script.Character;
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

        [SerializeField] private PlayerCharacter _playerCharacter;
        [SerializeField] private string startingScene = "PrototypeScene";
        [SerializeField] private List<string> dungeonLevels;

        // Track the main (or primary) currently loaded scene
        private string currentMainScene;

        // Keep track of all currently loaded additive scenes
        private List<string> loadedAdditiveScenes = new List<string>();

        private void Start()
        {
            // Initialize Service Locator
            _serviceLocator = ServiceLocator.Instance;

            // Ensure persistent objects like the player and manager
            MakePersistentObjects();

            // Optionally load the starting scene as the main scene
            if (!string.IsNullOrEmpty(startingScene))
            {
                StartCoroutine(LoadMainSceneAsync(startingScene));
            }
        }

        /// <summary>
        /// Returns the player character instance.
        /// </summary>
        public PlayerCharacter GetPlayer()
        {
            return _playerCharacter;
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
        public void AddScene(string sceneName)
        {
            // Avoid adding a scene that's already loaded
            if (loadedAdditiveScenes.Contains(sceneName))
            {
                Debug.LogWarning($"Scene {sceneName} is already loaded additively.");
                return;
            }

            StartCoroutine(AddSceneAsync(sceneName));
        }

        /// <summary>
        /// Unloads a currently loaded additive scene.
        /// </summary>
        public void UnloadAdditiveScene(string sceneName)
        {
            if (loadedAdditiveScenes.Contains(sceneName))
            {
                StartCoroutine(UnloadSceneAsync(sceneName));
            }
        }

        /// <summary>
        /// Unloads all currently loaded additive scenes.
        /// </summary>
        public void UnloadAllAdditiveScenes()
        {
            foreach (var scene in loadedAdditiveScenes)
            {
                StartCoroutine(UnloadSceneAsync(scene));
            }
        }

        /// <summary>
        /// Moves the player to a position in a specific target scene. Ensures the target scene is loaded additively.
        /// </summary>
        public void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {
            // Ensure the target scene is loaded additively
            if (!SceneManager.GetSceneByName(targetScene).isLoaded)
            {
                AddScene(targetScene);
            }

            // Move the player to the new spawn position
            _playerCharacter.transform.position = spawnPosition;
        }

        /// <summary>
        /// Reloads the current main scene (useful for restarting levels).
        /// </summary>
        public void ReloadCurrentScene()
        {
            if (!string.IsNullOrEmpty(currentMainScene))
            {
                StartCoroutine(ReloadMainSceneAsync(currentMainScene));
            }
        }

        /// <summary>
        /// Loads all dungeon levels additively.
        /// </summary>
        public void LoadDungeonLevels()
        {
            StartCoroutine(LoadDungeonLevelsCoroutine());
        }

        /// <summary>
        /// Unloads all dungeon levels.
        /// </summary>
        public void UnloadAllDungeonLevels()
        {
            foreach (var level in dungeonLevels)
            {
                UnloadAdditiveScene(level);
            }
            Debug.Log("All dungeon levels unloaded.");
        }

        /// <summary>
        /// Ensure persistent objects like the player are not destroyed across scenes.
        /// </summary>
        private void MakePersistentObjects()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(_playerCharacter.gameObject);
        }

#if ODIN_INSPECTOR
        [BoxGroup("Debug Controls")]
        [Button(ButtonSizes.Medium)]
        private void DebugLoadDungeonLevels()
        {
            LoadDungeonLevels();
        }

        [BoxGroup("Debug Controls")]
        [Button(ButtonSizes.Medium)]
        private void DebugUnloadDungeonLevels()
        {
            UnloadAllDungeonLevels();
        }
#endif

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

        private IEnumerator LoadDungeonLevelsCoroutine()
        {
            foreach (var level in dungeonLevels)
            {
                Debug.Log($"Loading dungeon level: {level}");
                yield return AddSceneAsync(level);
            }

            Debug.Log("All dungeon levels loaded additively.");
        }

        #endregion
    }
}
