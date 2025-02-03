// Author : Peiyu Wang @ Daphatus
// 01 02 2025 02 09

using System;
using _Script.Alchemy.PotionInstance;
using _Script.Character;
using _Script.Items;
using UnityEngine;

namespace _Script.Alchemy
{
    [CreateAssetMenu(fileName = "PotionBase", menuName = "Items/Alchemy/PotionBase")]
    public class PotionBase : ConsumableItem
    {
        public PotionEffect potionEffect;

        public override bool Use(PlayerCharacter playerCharacter)
        {
            playerCharacter.PotionEffectManager.ApplyPotionEffect(new PotionInstance.PotionInstance(this));
            return true;
        }
    }

    [Serializable]
    public class PotionEffect
    {
        public PotionType potionType;
        public int effectValue;
        public int duration;
    }
    
}