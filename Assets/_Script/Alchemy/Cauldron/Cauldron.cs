// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 42

using _Script.Character;
using _Script.Interactable;
using UnityEngine;

namespace _Script.Alchemy.Cauldron
{
    public class Cauldron : MonoBehaviour, IInteractable
    {
        //inventory
        
        //当玩家与其交互
        //加载配方书
        //加载炼金台
        public void Interact(PlayerCharacter player)
        {
            //player.RecipeBook
            //player.Inventory
        }

        public void InteractEnd()
        {
            throw new System.NotImplementedException();
        }

        public void OnHighlight()
        {
        }

        public void OnHighlightEnd()
        {
        }
    }
}