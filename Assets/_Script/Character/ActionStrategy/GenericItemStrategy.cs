using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using _Script.Map;
using _Script.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Script.Character.ActionStrategy
{
    public sealed class GenericStrategy : MonoBehaviour, IActionStrategy
    {
        /**
         * Consider this as temporary solution
         */
        [SerializeField] private Transform itemSlot;
        [SerializeField] private GameObject itemInHandPrefab;
        private static GameObject currentItem;
        private static SpriteRenderer currentSpriteRenderer;
        
        public void LeftMouseButtonDown(Vector3 direction)
        {
            Debug.Log("Left Mouse Button Down");
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
            Debug.Log("Left Mouse Button Up");
        }
        
        private void Update()
        {
            OnUpdatePosition();
            CheckingTargetInteraction();
        }

        private void CheckingTargetInteraction()
        {
            Debug.Log(GameTileMap.Instance.PointedTileType);
        }
        
        private void OnUpdatePosition()
        {
            // if has a weapon, rotate the weapon to face the mouse
            if (currentItem)
            {
                // get mouse position

                // convert screen position to world position
                if (Camera.main)
                {
                    Vector3 mousePosition = Mouse.current.position.ReadValue();
                    // convert screen position to world position
                    var worldPosition = Camera.main.ScreenToWorldPoint(mousePosition);
                    // calculate direction and distance
               
                    Vector3 direction = (worldPosition - transform.position) * 3f;
                    
                    if(direction.magnitude > 2f)
                    {
                        direction = direction.normalized * 2f;
                    }
                    
                    Vector3 targetPosition =  itemSlot.position + direction;
                    

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