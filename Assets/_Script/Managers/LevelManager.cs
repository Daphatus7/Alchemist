using System.Collections;
using System.Collections.Generic;
using _Script.Character;
using _Script.Map.WorldMap.MapNode;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.SceneManagement;
using Pathfinding; // Assuming AstarPath is in this namespace

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace _Script.Managers
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] private AstarPath _astarPath;
        [SerializeField] private GameObject _spawnBonfirePrefab;
        [SerializeField] private string _townScene = "TownMap";

        private PlayerCharacter _playerCharacter;
        private string _startingScene;

        private string currentMainScene;
        private string currentAdditiveScene;
        private List<string> loadedAdditiveScenes = new List<string>();

        public void Initialize(PlayerCharacter playerCharacter, string startingScene)
        {
            _playerCharacter = playerCharacter;
            _startingScene = startingScene;
        }

        /// <summary>
        /// Loads a new scene as the main scene (non-additive).
        /// Unloads any previously loaded main scene and all additive scenes.
        /// </summary>
        public void LoadMainScene(string sceneName)
        {
            if (sceneName == currentMainScene) return;
            StartCoroutine(LoadMainSceneAsync(sceneName));
        }

        /// <summary>
        /// Loads a scene additively.
        /// Before loading a new additive scene, unload the currently loaded additive scene if exists.
        /// After the additive scene is loaded and map generated, unload the main scene.
        /// </summary>
        public void LoadSelectedScene(NodeData nodeData)
        {
            // Unload the current additive scene before loading a new one
            if (!string.IsNullOrEmpty(currentAdditiveScene))
                UnloadAdditiveScene(currentAdditiveScene);

            currentAdditiveScene = nodeData.MapName;
            StartCoroutine(AddSceneAsync(nodeData));
        }

        public void UnloadCurrentAdditiveScene()
        {
            if (currentAdditiveScene != null)
                UnloadAdditiveScene(currentAdditiveScene);
            else
                Debug.LogWarning("No current additive scene to unload");
        }

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

        #region Scene Loading/Unloading Coroutines

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

            // Actually load the main scene additively here if needed:
            var asyncLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Loading main scene {sceneName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            currentMainScene = sceneName;
            Debug.Log($"Main scene {sceneName} has been loaded.");
        }

        private IEnumerator AddSceneAsync(NodeData nodeData)
        {
            var asyncLoad = SceneManager.LoadSceneAsync(nodeData.MapName, LoadSceneMode.Additive);
            
            while (!asyncLoad.isDone)
            {
                Debug.Log($"Loading additive scene {nodeData.MapName}: {asyncLoad.progress * 100}%");
                yield return null;
            }

            Scene loadedScene = SceneManager.GetSceneByName(nodeData.MapName);
            if (loadedScene.IsValid())
            {
                SceneManager.SetActiveScene(loadedScene);
            }

            loadedAdditiveScenes.Add(nodeData.MapName);
            Debug.Log($"Additive scene {nodeData.MapName} has been loaded.");
            
            if (!string.IsNullOrEmpty(currentMainScene))
            {
                yield return UnloadSceneAsyncWait(currentMainScene);
                currentMainScene = null;
            }
            
            OnMapFinishedLoading(nodeData);
        }

        
        private IEnumerator UnloadSceneAsyncWait(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
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
        private IEnumerator UnloadSceneAsync(string sceneName)
        {
            if (SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                // If unloading the active scene and we have a currentAdditiveScene loaded, set it active
                if (SceneManager.GetActiveScene().name == sceneName && !string.IsNullOrEmpty(currentAdditiveScene))
                {
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(currentAdditiveScene));
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

        private void UnloadAdditiveScene(string sceneName)
        {
            if (loadedAdditiveScenes.Contains(sceneName))
            {
                currentAdditiveScene = null;
                StartCoroutine(UnloadSceneAsync(sceneName));
            }
        }

        private void UnloadAllAdditiveScenes()
        {
            foreach (var scene in loadedAdditiveScenes.ToArray())
            {
                StartCoroutine(UnloadSceneAsync(scene));
            }
        }

        #endregion

        #region Map Generation and Setup

        private void OnMapFinishedLoading(NodeData nodeData)
        {
            StartCoroutine(OnMapFinishedLoadingRoutine(nodeData));
        }
        
        private IEnumerator OnMapFinishedLoadingRoutine(NodeData nodeData)
        {
            if (SubGameManager.Instance.GenerateMap(nodeData, out Vector2Int spawnPoint, out Vector2Int endPoint))
            {
                // Wait for the map to be generated
                yield return null;
        
                var graph = _astarPath.data.gridGraph;
                graph.nodeSize = 0.5f;
                graph.width = SubGameManager.Instance.MapSize.x * 2;
                graph.depth = SubGameManager.Instance.MapSize.y * 2;

                float totalWidth = graph.width * graph.nodeSize;
                float totalDepth = graph.depth * graph.nodeSize;

                graph.center = new Vector3(totalWidth / 2f, totalDepth / 2f, 0);
                graph.SetDimensions(graph.width, graph.depth, graph.nodeSize);
                AstarPath.active.Scan();

                MovePlayerToScene(new Vector3(spawnPoint.x, spawnPoint.y, 0), nodeData.MapName);
                Instantiate(_spawnBonfirePrefab, new Vector3(endPoint.x, endPoint.y, 0), Quaternion.identity);
            }
        }
        #endregion
    }
}