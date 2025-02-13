// Author : Peiyu Wang @ Daphatus
// 13 02 2025 02 12

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Script.UserInterface.InformationBoard
{
    public class InformationBoardUI : MonoBehaviour
    {
        private void OnEnable()
        {
            InformationBoard.Instance.onDisplayNewContext += OnInformationAdded;
            InformationBoard.Instance.onRemoveContext += RemoveInformation;
        }
        
        private void OnDisable()
        {
            InformationBoard.Instance.onDisplayNewContext -= OnInformationAdded;
            InformationBoard.Instance.onRemoveContext -= RemoveInformation;
        }
        
        private void OnInformationAdded(InformationContext context)
        {
            //display different types of information based on the context
        }
        
        private void RemoveInformation(InformationContext context)
        {
            //remove information
        }
    }
}