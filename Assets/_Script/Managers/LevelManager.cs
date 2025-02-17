using System.Collections;
using System.Collections.Generic;
using _Script.Character;
using _Script.Map.WorldMap;
using _Script.Map.WorldMap.MapNode;
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
        private string _startingScene;

        private string _currentMainScene;
        private string _currentAdditiveScene;
        private readonly List<string> _loadedAdditiveScenes = new List<string>();
        private AstarPath _astarPath;
        /// <summary>
        /// Initialize references such as the PlayerCharacter and starting scene.
        /// Called once from GameManager.
        /// </summary>
        public void Initialize(PlayerCharacter playerCharacter, string startingScene, AstarPath astarPath)
        {
            _playerCharacter = playerCharacter;
            _startingScene = startingScene;
            _astarPath = astarPath;
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
        public void LoadSelectedScene(NodeDataInstance nodeDataInstance)
        {
            // Unload the existing additive scene if any
            if (!string.IsNullOrEmpty(_currentAdditiveScene))
            {
                UnloadAdditiveScene(_currentAdditiveScene);
            }
            _currentAdditiveScene = nodeDataInstance.MapName;
            GameManager.Instance.StartCoroutine(AddSceneAsync(nodeDataInstance));
        }

        /// <summary>
        /// Unloads the current additive scene (if any).
        /// </summary>
        public void UnloadCurrentAdditiveScene()
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
        public void MovePlayerToScene(Vector3 spawnPosition, string targetScene)
        {
            if (_loadedAdditiveScenes.Contains(targetScene) && _playerCharacter != null)
            {
                _playerCharacter.transform.position = spawnPosition;
                Debug.Log($"Player moved to {spawnPosition} in scene {targetScene}.");
            }
            else
            {
                Debug.LogWarning($"Scene '{targetScene}' not loaded or PlayerCharacter is null. Cannot move player.");
            }

            // Correctly unsubscribe using the named method!
            SubGameManager.Instance.OnLevelGenerated -= OnSubGameManagerLevelGenerated;
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
            while (!asyncLoad.isDone)
            {
               // Debug.Log($"Loading main scene {sceneName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            _currentMainScene = sceneName;
            //Debug.Log($"Main scene '{sceneName}' loaded.");
        }

        private IEnumerator AddSceneAsync(NodeDataInstance nodeDataInstance)
        {
            var mapName = nodeDataInstance.MapName;
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
            
            // Trigger generation in the SubGameManager
            if (SubGameManager.Instance.LoadNextLevel(nodeDataInstance))
            {
                SubGameManager.Instance.OnLevelGenerated += OnSubGameManagerLevelGenerated;
            }
            else
            {
                _astarPath.Scan();
                MovePlayerToScene(SubGameManager.Instance.SpawnPoint.position, _currentAdditiveScene);
            }
            // Subscribe using a NAMED method, not an anonymous delegate
        }

        /// <summary>
        /// Named method for the OnLevelGenerated event.
        /// Subscribing/unsubscribing with the same method ensures correct event handling.
        /// </summary>
        private void OnSubGameManagerLevelGenerated()
        {
            // Use SubGameManager's known spawn position or nodeData's mapName if needed.
            // Suppose the SubGameManager has a public SpawnPoint property:
            var spawnPoint = SubGameManager.Instance.SpawnPoint.position;
            var targetScene = _currentAdditiveScene;
            
            var gridGraph = AstarPath.active.data.gridGraph;

            if (gridGraph != null)
            {
                // Set the size of the graph
                gridGraph.center = SubGameManager.Instance.MapCenter;
                gridGraph.width = SubGameManager.Instance.MapBounds.x * 2;
                gridGraph.depth = SubGameManager.Instance.MapBounds.y * 2;
                gridGraph.nodeSize = 0.5f;        // Size of each node in world units

                // Optionally adjust boundaries
                gridGraph.UpdateSizeFromWidthDepth();
            }
            
            _astarPath.Scan();
            MovePlayerToScene(spawnPoint, targetScene);
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