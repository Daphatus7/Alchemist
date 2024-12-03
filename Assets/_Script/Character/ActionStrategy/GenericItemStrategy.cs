using _Script.Inventory.ActionBarBackend;
using _Script.Items;
using _Script.Map;
using _Script.Utilities;
using UnityEngine;

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
            if (IsCursorInRange())
            {
                _useItem?.UseItem();
            }
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
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
            OnUpdatePosition();
        }

        private void CheckingTargetInteraction(Vector2 pos)
        {
            //TODO: Implement this - potentially improve the performance
            var tile = GameTileMap.Instance.PointedTile;
        }
        
        
        
        
        private void OnUpdatePosition()
        {
            // if has item in hand
            if (currentItem)
            {
                
                // convert screen position to world position
                if (CursorMovementTracker.HasCursorMoved)
                {
                    //if can show the preview that means the player has landed on a fertile soil
                    if (IsCursorInRange())
                    {
                        if (ShowSeedPreview()) return;
                    }
                    else
                    {
                        UpdateItemPosition();
                    }
                }
            }
        }

        private bool IsCursorInRange()
        {
            var distance = Vector3.Distance(CursorMovementTracker.CursorPosition, itemSlot.position);
            return distance <= itemDistance;
        }
        
        private bool ShowSeedPreview()
        {
            //if the item is not a seed, return false
            if(_useItem.ItemData.ItemTypeString != "Seed") return false;
            
            //cast the item to seed item
            var seedItem = _useItem.ItemData as SeedItem;
            if(!seedItem) return false;
            
            //get the pointed tile
            var tile = GameTileMap.Instance.PointedTile;
            
            //if the tile is not null and is fertile, show the preview
            if (tile != null && tile.IsFertile)
            {
                // show preview
                currentSpriteRenderer.sprite = seedItem.seedOnGroundSprite;
                currentItem.transform.position = GameTileMap.Instance.GetTileWorldCenterPosition(tile.Position.x, tile.Position.y);
                return true;
            }
            else
            {
                return false;
            }
        }
        
        
        private void UpdateItemPosition()
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
        
        private ActionBarContext _useItem;
        
        public void ChangeItem(ActionBarContext useItem)
        {
            // Spawn
            _useItem = useItem;
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
            currentSpriteRenderer.sprite = useItem.ItemData.ItemSprite;
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