// Author : Peiyu Wang @ Daphatus
// 07 12 2024 12 42

using _Script.Map;
using _Script.Utilities;
using _Script.Inventory.ActionBarBackend;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    public abstract class BaseItemStrategy : BaseActionStrategy
    {
        [SerializeField] protected Transform itemSlot;
        [SerializeField] protected GameObject itemInHandPrefab;
        [SerializeField] protected float itemDistance = 1f;

        protected ActionBarContext currentUseItem;
        protected GameObject currentItem;
        protected SpriteRenderer currentSpriteRenderer;

        protected virtual void OnEnable()
        {
            if (GameTileMap.Instance != null)
                GameTileMap.Instance.OnCursorMoved += OnCursorMoved;
        }

        protected virtual void OnDisable()
        {
            if (GameTileMap.Instance != null)
                GameTileMap.Instance.OnCursorMoved -= OnCursorMoved;
        }

        protected virtual void Update()
        {
            UpdateItemPositionIfNeeded();
        }

        public override void ChangeItem(ActionBarContext useItem)
        {
            currentUseItem = useItem;

            if (currentItem == null)
            {
                currentItem = Instantiate(itemInHandPrefab, itemSlot.transform.position, Quaternion.identity, itemSlot);
                currentSpriteRenderer = currentItem.GetComponent<SpriteRenderer>();
            }
            else
            {
                currentItem.SetActive(true);
            }

            if (currentSpriteRenderer != null && useItem.ItemData != null)
            {
                currentSpriteRenderer.sprite = useItem.ItemData.ItemSprite;
            }

            OnItemChanged(useItem);
        }

        public override void RemoveItem()
        {
            if (currentItem)
            {
                currentItem.SetActive(false);
            }
            OnItemRemoved();
        }

        public override void LeftMouseButtonDown(Vector3 direction)
        {
            if (IsCursorInRange())
            {
                currentUseItem?.UseItem();
            }
        }

        public override void LeftMouseButtonUp(Vector3 direction)
        {
            // By default no action. Override if needed.
        }

        protected virtual void OnItemChanged(ActionBarContext useItem)
        {
            // Override in subclass if special initialization is needed
        }

        protected virtual void OnItemRemoved()
        {
            // Remove inventory item 
        }

        protected virtual void OnCursorMoved(Vector2 pos)
        {
            // Override in subclass if special cursor-based logic needed
        }

        protected bool IsCursorInRange()
        {
            if (itemSlot == null) return false;
            var distance = Vector3.Distance(CursorMovementTracker.CursorPosition, itemSlot.position);
            return distance <= itemDistance;
        }

        protected virtual void UpdateItemPositionIfNeeded()
        {
            if (currentItem && CursorMovementTracker.HasCursorMoved)
            {
                if (!TryShowPreview())
                {
                    UpdateItemPosition();
                }
            }
        }

        protected virtual bool TryShowPreview()
        {
            // By default, no preview. Subclasses override if needed.
            return false;
        }

        protected void UpdateItemPosition()
        {
            if (itemSlot == null) return;
            Vector3 mousePosition = CursorMovementTracker.CursorPosition;
            Vector3 direction = (mousePosition - itemSlot.position);
            var extent = Mathf.Min(direction.magnitude, itemDistance);
            Vector3 targetPosition = itemSlot.position + direction.normalized * extent;
            if (currentItem != null)
            {
                currentItem.transform.position = new Vector3(targetPosition.x, targetPosition.y, 0);
            }
        }
    }
}
