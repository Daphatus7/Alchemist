// Author : Peiyu Wang @ Daphatus
// 27 12 2024 12 00
using UnityEngine;

namespace _Script
{
    /// <summary>
    /// Central place to store all layer indices and utility methods,
    /// so we don't rely on magic numbers throughout the code.
    /// </summary>
    public static class LayerManager
    {
        public const int Default          = 0;
        public const int TransparentFX    = 1;
        public const int IgnoreRaycast    = 2;
        public const int Minimap          = 3;
        public const int Water            = 4;
        public const int UI               = 5;
        public const int Player           = 6;
        public const int NPCLayer         = 7;
        public const int Enemy            = 8;
        public const int Interactable     = 9;
        public const int Obstacle         = 10;
        public const int Tools            = 20;
        public const int Projectiles      = 21;
        public const int FX               = 22;
        public const int Background       = 31;

        // --- Helper for bitmask usage (e.g. Physics overlap/raycast) ---
        // Example usage: Physics2D.Raycast(origin, dir, distance, LayerMask.GetMask(LayerManager.PlayerMaskName));
        public static int PlayerMask => 1 << Player;
        public static int EnemyMask  => 1 << Enemy;
        
        public static LayerMask EnemyLayerMask => 1 << Enemy;
        // etc. Add as needed

        // --- If you prefer string-based for LayerMask.GetMask(...) ---
        public static string PlayerMaskName => "Player";
        public static string EnemyMaskName  => "Enemy";

        /// <summary>
        /// Sets the layer of a GameObject and all its children recursively.
        /// </summary>
        public static void SetLayerRecursively(GameObject obj, int newLayer)
        {
            if (obj == null) return;
            obj.layer = newLayer;

            foreach (Transform child in obj.transform)
            {
                if (child == null) continue;
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }

        /// <summary>
        /// A quick helper to fetch the layer index from a layer name. 
        /// If not found, returns -1.
        /// </summary>
        public static int GetLayerIndex(string layerName)
        {
            int layer = LayerMask.NameToLayer(layerName);
            if (layer < 0)
            {
                Debug.LogWarning($"Layer '{layerName}' not found in Project Settings.");
            }
            return layer;
        }
    }
}