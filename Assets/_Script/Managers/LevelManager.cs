using System.Collections;
using System.Collections.Generic;
using _Script.Character;
using _Script.Map.MapLoadContext;
using _Script.Map.MapLoadContext.ContextInstance;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Script.Managers
{
    /// <summary>
    /// A non-static class that the GameManager holds.
    /// Works as a factory to initialize scenes (main or additive) and manage transitions.
    /// </summary>
    public class LevelManager
    {
        private PlayerCharacter _playerCharacter;

        public PlayerCharacter PlayerCharacter
        {
            get
            {
                if(!_playerCharacter)
                {
                    _playerCharacter = GameManager.Instance.PlayerCharacter;
                    
                    if(!_playerCharacter)
                    {
                        Debug.LogWarning("PlayerCharacter is null, event after trying get it from GameManager.");
                    }
                }
                return _playerCharacter;
            }
        }

        private string _currentMainScene;
        private string _currentAdditiveScene;
        private readonly List<string> _loadedAdditiveScenes = new List<string>();
        /// <summary>
        /// Initialize references such as the PlayerCharacter and starting scene.
        /// Called once from GameManager.
        /// </summary>
        public void Initialize(PlayerCharacter playerCharacter)
        {
            _playerCharacter = playerCharacter;
        }

        /// <summary>
        /// Loads the main scene (non-additive).
        /// Unloads any existing main scene or additive scenes.
        /// </summary>
        public void LoadMainScene(string sceneName)
        {
            GameManager.Instance.StartCoroutine(LoadMainSceneAsync(sceneName));
        }

        /// <summary>
        /// Loads a scene additively. Before loading a new additive scene,
        /// we unload the current additive scene if it exists.
        /// After loading, we might unload the main scene to fully swap.
        /// </summary>
        internal void LoadSelectedScene(MapLoadContextInstance instance)
        {
            // Unload the existing additive scene if any
            if (!string.IsNullOrEmpty(_currentAdditiveScene))
            {
                UnloadAdditiveScene(_currentAdditiveScene);
            }
            _currentAdditiveScene = instance.MapName;
            GameManager.Instance.StartCoroutine(AddSceneAsync(instance));
        }

        /// <summary>
        /// Unloads the current additive scene (if any).
        /// </summary>
        internal void UnloadCurrentAdditiveScene()
        {
            if (!string.IsNullOrEmpty(_currentAdditiveScene))
            {
                UnloadAdditiveScene(_currentAdditiveScene);
            }
            else
            {
                Debug.LogWarning("No current additive scene to unload!");
            }
        }

        /// <summary>
        /// Moves the player to a spawn position in a target scene (if that scene is loaded).
        /// Also unsubscribes from OnLevelGenerated if we were subscribed.
        /// </summary>
        private void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {
            if (_loadedAdditiveScenes.Contains(targetScene))
            {
                if (PlayerCharacter)
                {
                    _playerCharacter.transform.position = spawnPosition;
                    Debug.Log($"Player moved to {spawnPosition} in scene {targetScene}.");
                }
                else
                {
                    Debug.Log($"character is null");
                }

            }
            else
            {
                Debug.Log($"Scene '{targetScene}' not loaded or PlayerCharacter is null. Cannot move player.");
            }
        }

        #region Async Scene Loading Coroutines

        private IEnumerator LoadMainSceneAsync(string sceneName)
        {
            // 1) Unload the current main scene if present
            if (!string.IsNullOrEmpty(_currentMainScene))
            {
                yield return SceneManager.UnloadSceneAsync(_currentMainScene);
                _currentMainScene = null;
            }

            // 2) Unload all additive scenes
            foreach (var scene in _loadedAdditiveScenes.ToArray())
            {
                yield return SceneManager.UnloadSceneAsync(scene);
                _loadedAdditiveScenes.Remove(scene);
            }
            _currentAdditiveScene = null;

            // 3) Load the new main scene additively
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (asyncLoad is { isDone: false })
            {
               // Debug.Log($"Loading main scene {sceneName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            _currentMainScene = sceneName;
        }

        private IEnumerator AddSceneAsync(MapLoadContextInstance instance)
        {
            var mapName = instance.MapName;
            // Load the additive scene
            var asyncLoad = SceneManager.LoadSceneAsync(mapName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Loading additive scene {mapName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            _loadedAdditiveScenes.Add(mapName);
            Debug.Log($"Additive scene '{mapName}' loaded.");

            // Optionally, unload the main scene after the new additive scene is up
            if (!string.IsNullOrEmpty(_currentMainScene))
            {
                yield return SceneManager.UnloadSceneAsync(_currentMainScene);
                _currentMainScene = null;
            }

            SubGameManager.Instance.LoadLevelContent(instance);
        }
        
        private void UnloadAdditiveScene(string sceneName)
        {
            GameManager.Instance.StartCoroutine(UnloadAdditiveSceneAsync(sceneName));
        }

        private IEnumerator UnloadAdditiveSceneAsync(string sceneName)
        {
            if (_loadedAdditiveScenes.Contains(sceneName))
            {
                yield return SceneManager.UnloadSceneAsync(sceneName);
                _loadedAdditiveScenes.Remove(sceneName);
                Debug.Log($"Additive scene '{sceneName}' unloaded.");
            }
        }

        #endregion
    }
}