using System;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Map;
using _Script.Map.Tile.Tile_Base;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.ActionStrategy
{
    [DefaultExecutionOrder(50)]
    public sealed class GenericItemStrategy : MonoBehaviour, IActionStrategy
    {
        /**
         * Consider this as temporary solution
         */
        [SerializeField] private Transform itemSlot;
        [SerializeField] private GameObject itemInHandPrefab;
        private GameObject currentItem;
        private SpriteRenderer currentSpriteRenderer;
        [SerializeField] private float itemDistance = 1f;
        
        public void LeftMouseButtonDown(Vector3 direction)
        {
            Debug.Log("Left Mouse Button Down");
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
            Debug.Log("Left Mouse Button Up");
        }

        private void OnEnable()
        {
            GameTileMap.Instance.OnCursorMoved += CheckingTargetInteraction;
        }

        private void OnDisable()
        {
            GameTileMap.Instance.OnCursorMoved -= CheckingTargetInteraction;
        }
        
        private void Update()
        {
            Debug.Log("Generic Item Strategy Update");
            OnUpdatePosition();
        }

        private void CheckingTargetInteraction(Vector2 pos)
        {
            var tile = GameTileMap.Instance.PointedTile;
        }
        
        private void OnUpdatePosition()
        {
            // if has a weapon, rotate the weapon to face the mouse
            if (currentItem)
            {
                // get mouse position

                // convert screen position to world position
                if (CursorMovementTracker.HasCursorMoved)
                {
                    
                    Vector3 mousePosition = CursorMovementTracker.CursorPosition;
                    // convert screen position to world position
                    // calculate direction and distance
               
                    Vector3 direction = (mousePosition - itemSlot.position);
                    var extent = Mathf.Min(direction.magnitude, itemDistance);
                    Vector3 targetPosition = itemSlot.position + direction.normalized * extent;

                    // update item position
                    currentItem.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
                }
            }
        }
        
        
        public void ChangeItem(ItemData itemData)
        {
            // Spawn
            //if the item is spawned but not enabled, enable it
            if (currentItem)
            {
                currentItem.SetActive(true);
            }
            else
            {
                currentItem = Instantiate(itemInHandPrefab, itemSlot.transform.position, Quaternion.identity);
                currentItem.transform.parent = itemSlot.transform;
                currentSpriteRenderer = currentItem.GetComponent<SpriteRenderer>();
            } 
            //set renderer
            currentSpriteRenderer.sprite = itemData.ItemSprite;
        }
        
        public void RemoveItem()
        {
            if(currentItem)
            {
                currentItem.SetActive(false);
            }
        }
    }

}