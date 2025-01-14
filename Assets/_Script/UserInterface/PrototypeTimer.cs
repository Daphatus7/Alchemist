// Author : Peiyu Wang @ Daphatus
// 14 01 2025 01 24

using System;
using TMPro;
using UnityEngine;

namespace _Script.UserInterface
{
    public class PrototypeTimer : MonoBehaviour
    {
        private float _time;
        private TextMeshProUGUI _text;

        private void Start()
        {
            _text = GetComponent<TextMeshProUGUI>();
        }

        private void Update()
        {
            _time += Time.deltaTime;
            _text.text = $"Time: {_time:F2}";
        }
    }
}