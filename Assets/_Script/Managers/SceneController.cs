// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 27

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Script.Managers
{
    public class SceneController : Singleton<SceneController>
    {
        [SerializeField] private List<string> sceneNames;
        
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }

        public void ReloadCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}