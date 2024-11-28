using _Script.Inventory.InventoryFrontend;
using _Script.Items;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button slotButton;
        [SerializeField] private Image highlight;

        private IContainerUIHandle _inventoryUI;
        private int _slotIndex;  public int SlotIndex => _slotIndex;

        /**
         * only visual representation of the item
         */
        private InventoryItem currentItem;

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            
            //hide
            highlight.enabled = false;
        }

        private void OnEnable()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(OnSlotClicked);
            }
        }

        private void OnDisable()
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveListener(OnSlotClicked);
            }
        }

        public void SetSlot(InventoryItem item)
        {
            currentItem = item;

            if (item != null && item.Icon != null)
            {
                icon.enabled = true;
                icon.sprite = item.Icon;
                icon.color = Color.white;
                quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }

        public virtual void ClearSlot()
        {
            currentItem = null;
            icon.color = new Color(1, 1, 1, 0);
            icon.enabled = false;
            quantityText.text = "";
        }

        public virtual void OnSlotClicked()
        {
            Debug.Log("Slot clicked." + _slotIndex);
            _inventoryUI.OnSlotClicked(this);
        }

        #region Drag and Drop

        [SerializeField] private GameObject dragItemPrefab;
        private static GameObject dragItem;
        private static Canvas canvas;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        
        public void HighlightSlot()
        {
            highlight.enabled = true;
        }
        
        public void UnhighlightSlot()
        {
            highlight.enabled = false;
        }
        
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (currentItem != null)
            {
                icon.raycastTarget = false;
                if(dragItem == null)
                {
                    dragItem = Instantiate(dragItemPrefab, canvas.transform);
                }
                else
                {
                    dragItem.SetActive(true);
                }

                Image dragItemImage = dragItem.GetComponent<Image>();
                if (dragItemImage != null)
                {
                    dragItemImage.sprite = icon.sprite;
                    dragItemImage.raycastTarget = false; // Disable raycast target
                }

                SetDragItemPosition(eventData);

                icon.color = new Color(1, 1, 1, 0);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragItem != null)
            {
                SetDragItemPosition(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragItem != null)
            {
                dragItem.SetActive(false);
                icon.color = Color.white;
            }
            icon.raycastTarget = true;
        }

        private void SetDragItemPosition(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            dragItem.transform.localPosition = localPoint;
        }

        public void OnDrop(PointerEventData eventData)
        {
            InventorySlotDisplay sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotDisplay>();
            
            //if there is a inventory slot display when dropping
            if (sourceSlot != null && sourceSlot != this)
            {
                //first check if the item can be swapped
                /*
                 * Cases where the item cannot be swapped
                 * 1. if the target inventory is equipment inventory
                 *  a. if the source is in equipment inventory
                 *      i. they are both in the same equipment slot, check if the item type is the same
                 *      ii. they are in different equipment slot, then return fail
                 *  b. the source is not in other inventory, 
                 */
                
                //TODO: Implement the swapping logic special cases (Equipment)
                
                
                //Swap items
                var myItem = _inventoryUI.RemoveAllItemsFromSlot(_slotIndex);
                var sourceItem = sourceSlot._inventoryUI.RemoveAllItemsFromSlot(sourceSlot._slotIndex);
                _inventoryUI.AddItemToEmptySlot(sourceItem, _slotIndex);
                sourceSlot._inventoryUI.AddItemToEmptySlot(myItem, sourceSlot._slotIndex);
            }
            //if there is no inventory slot display when dropping, drop to the ground
            else if (sourceSlot == null)
            {
                //Drop item to the ground
            }
            
            //hide the drag item visual
            if (dragItem != null)
            {
                dragItem.SetActive(false);
            }
            
        }
        
        #endregion
    }
}
