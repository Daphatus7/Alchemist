﻿using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Edgar.Unity
{
    /// <summary>
    /// Basic dungeon room template initializer.
    /// Uses DungeonTilemapLayersHandler to create tilemaps structure.
    /// </summary>
    public class DungeonRoomTemplateInitializerGrid2D : RoomTemplateInitializerBaseGrid2D
    {
        
        protected override void InitializeTilemaps(GameObject tilemapsRoot)
        {
            var tilemapLayersHandlers = new DungeonTilemapLayersHandlerGrid2D();
            tilemapLayersHandlers.InitializeTilemaps(tilemapsRoot);
        }

        #if UNITY_EDITOR
        [MenuItem("Assets/Create/Edgar/Dungeon room template")]
        public static void CreateRoomTemplatePrefab()
        {
            Debug.Log("Creating dungeon room template");
            RoomTemplateInitializerUtilsGrid2D.CreateRoomTemplatePrefab<DungeonRoomTemplateInitializerGrid2D>();
        }
        #endif
    }
}