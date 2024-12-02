using System;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Inventory
{
    public class DragItem : MonoBehaviour
    {
        private Image _image;
        private void Awake()
        {
            _image = GetComponent<Image>();
        }
        
        public void SetImage(Sprite sprite)
        {
            _image.sprite = sprite;
        }
    }
}