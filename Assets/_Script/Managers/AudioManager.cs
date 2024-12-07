// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 02

using UnityEngine;

namespace _Script.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        public AudioSource backgroundMusic;
        public AudioSource soundEffects;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void PlaySound(AudioClip clip)
        {
            soundEffects.PlayOneShot(clip);
        }
    }

}