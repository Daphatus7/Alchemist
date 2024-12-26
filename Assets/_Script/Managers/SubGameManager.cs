using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Diagnostics;
using Edgar.Unity;
using Edgar.Unity.Examples;
using Edgar.Unity.Examples.Example2;
using Debug = UnityEngine.Debug;

namespace _Script.Managers
{
    public class SubGameManager : GameManagerBase<SubGameManager>
    {
        [Header("Optional: Dungeon Generation Example")]
        [SerializeField] private DungeonGeneratorGrid2D _dungeonGenerator;
        
        public event System.Action OnLevelGenerated;

        public override void LoadNextLevel()
        {
            ShowLoadingScreen("SubGameManager", "Generating level...");

            if (_dungeonGenerator == null)
            {
                Debug.LogWarning("No DungeonGenerator assigned to this SubGameManager!");
                HideLoadingScreen();
                return;
            }

            StartCoroutine(GenerateLevelCoroutine());
        }

        protected override void SingletonAwake()
        {
            // If you need something in Awake
        }

        private IEnumerator GenerateLevelCoroutine()
        {
            // Ensure this SubGameManager's scene is the active scene
            Scene myScene = gameObject.scene;    // The scene containing the SubGameManager MonoBehaviour
            
            SceneManager.SetActiveScene(myScene);

            // 2. Wait one frame so the scene switch can fully register
            yield return null;
            
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            // Wait one frame so the loading screen becomes visible
            yield return null;

            // Now generate the dungeon
            var payload = _dungeonGenerator.Generate();
            
            // Wait another frame to let newly spawned objects initialize
            yield return null;

            stopwatch.Stop();
            double seconds = stopwatch.ElapsedMilliseconds / 1000d;

            SetLevelInfo($"Generated in {seconds:F2}s");
            HideLoadingScreen();

            OnLevelGenerated?.Invoke();
        }
    }
}