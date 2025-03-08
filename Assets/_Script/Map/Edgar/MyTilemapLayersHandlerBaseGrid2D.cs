// Author : Peiyu Wang @ Daphatus
// 07 03 2025 03 46

using System.Collections.Generic;
using Edgar.Unity;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _Script.Map.Edgar
{
    [CreateAssetMenu(menuName = "Dungeon generator/My Tilemap layers handler", fileName = "MyTilemapLayersHandlerBaseGrid2D")]
    public class MyTilemapLayersHandlerBaseGrid2D : TilemapLayersHandlerBaseGrid2D
    {
        [Header("Orchestrate the layers"), ReadOnly]
        public List<string> layers = new List<string>()
        {
            "Floor",
            "Walls",
            "Collideable",
            "Other 1",
            "Other 2",
            "Other 3"
        };
        
        public override void InitializeTilemaps(GameObject obj)
        {
            
            Debug.Log("Initializing tilemapsxxx");
            foreach(var layer in layers)
            {
                Debug.Log("Layer: " + layer);
            }
            // First make sure that you add the grid component
            var grid = obj.AddComponent<Grid>();

            // If we want a different cell size, we can configure that here
            // grid.cellSize = new Vector3(1, 2, 1);
            
            // And now we create individual tilemap layers
            var floorTilemapObject = CreateTilemapGameObject(layers[0], obj, 0);
            var wallsTilemapObject = CreateTilemapGameObject(layers[1], obj, 1);
            AddCompositeCollider(wallsTilemapObject);
            
            for(var i = 2; i < layers.Count; i++)
            {
                Debug.Log("Creating tilemap layer: " + layers[i]);
                CreateTilemapGameObject(layers[i], obj, i);
            }
        }

        /// <summary>
        /// Helper to create a tilemap layer
        /// </summary>
        protected GameObject CreateTilemapGameObject(string name, GameObject parentObject, int sortingOrder)
        {
            // Create a new game object that will hold our tilemap layer
            var tilemapObject = new GameObject(name);
            // Make sure to correctly set the parent
            tilemapObject.transform.SetParent(parentObject.transform);
            var tilemap = tilemapObject.AddComponent<Tilemap>();
            var tilemapRenderer = tilemapObject.AddComponent<TilemapRenderer>();
            tilemapRenderer.sortingOrder = sortingOrder;

            return tilemapObject;
        }

        /// <summary>
        /// Helper to add a collider to a given tilemap game object.
        /// </summary>
        protected void AddCompositeCollider(GameObject tilemapGameObject, bool isTrigger = false)
        {
            var tilemapCollider2D = tilemapGameObject.AddComponent<TilemapCollider2D>();
            tilemapCollider2D.usedByComposite = true;

            var compositeCollider2d = tilemapGameObject.AddComponent<CompositeCollider2D>();
            compositeCollider2d.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider2d.isTrigger = isTrigger;

            tilemapGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
    }
}