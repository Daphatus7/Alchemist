// Author : Peiyu Wang @ Daphatus
// 01 02 2025 02 56

using System;
using _Script.Character;
using UnityEngine;

namespace _Script.Alchemy
{
    
    /// <summary>
    /// All Apply instant effect on player 
    /// </summary>
    [CreateAssetMenu(fileName = "Potion", menuName = "Items/Alchemy/Potion")]
    public class Potion : PotionBase
    {
        public override bool Use(PlayerCharacter playerCharacter)
        {
            return true;
        }
    }
 
}