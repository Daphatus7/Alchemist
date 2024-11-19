using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.UI
{
    public class HeartUI : MonoBehaviour
    {
        [SerializeField] private Image heartImage;

        public void SetHeartFill(float fill)
        {
            heartImage.fillAmount = fill;
        }
    }
}