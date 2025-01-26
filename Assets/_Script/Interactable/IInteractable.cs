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
        /// <summary>
        /// When the player hit the mouse button
        /// </summary>
        /// <param name="player"></param>
        void Interact(PlayerCharacter player);
        
        /// <summary>
        /// When the player stops hitting the mouse button
        /// </summary>
        void InteractEnd();
        
        /// <summary>
        /// When the mouse hovers over the object
        /// </summary>
        void OnHighlight();
        
        /// <summary>
        ///  When the mouse stops hovering over the object
        /// </summary>
        void OnHighlightEnd();
    }
}