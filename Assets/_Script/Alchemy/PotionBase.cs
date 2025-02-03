// Author : Peiyu Wang @ Daphatus
// 01 02 2025 02 09

using System;
using _Script.Alchemy.PotionInstance;
using _Script.Items;
using UnityEngine;

namespace _Script.Alchemy
{
    [CreateAssetMenu(fileName = "PotionBase", menuName = "Items/Alchemy/PotionBase")]
    public abstract class PotionBase : ConsumableItem
    {
        public PotionEffect potionEffect;
    }

    [Serializable]
    public class PotionEffect
    {
        public PotionType potionType;
        public int effectValue;
        public int duration;
    }
    
}