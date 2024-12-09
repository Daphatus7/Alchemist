// /**
//  *
//  *   Author: daphatus
//  *   File: ${File.Name}
//  *   Date: $[InvalidReference]
//  */

using _Script.Character;
using UnityEngine;

namespace _Script.Interactable
{
    public interface IInteractable
    {
        void Interact(GameObject player);
        void InteractEnd(GameObject player);
        void OnHighlight();
        void OnHighlightEnd();
    }
}