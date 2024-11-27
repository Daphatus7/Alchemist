using _Script.Inventory.InventoryFrontend;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay: MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText; // Changed to TextMeshProUGUI
        [SerializeField] private Button slotButton;
        [SerializeField] private Image highlight;
        private IContainerUIHandle _inventoryUI;
        private int _slotIndex; public int SlotIndex => _slotIndex;
        
        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
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


        public void SetSlot(Items.InventoryItem item)
        {
            // Set icon and quantity
            if (item != null && item.Icon != null)
            {
                icon.sprite = item.Icon;
                icon.color = Color.white; // Make the icon fully visible
                quantityText.text = item.Quantity > 1 ? item.Quantity.ToString() : ""; // Only show quantity if more than 1
            }
            else
            {
                ClearSlot(); // Clear slot if item is null
            }
        }
        
        public virtual void ClearSlot()
        {
            // Clear the icon and hide the quantity
            icon.sprite = null;
            icon.color = new Color(1, 1, 1, 0); // Fully transparent
            quantityText.text = "";
        }

        public virtual void OnSlotClicked()
        {
            _inventoryUI.OnSlotClicked(this);
        }


        #region Drag and Drop

        [SerializeField] private GameObject dragItemPrefab;
        private GameObject dragItem;
        private Canvas canvas;  // Reference to the canvas

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();  // Ensure the item is under the correct canvas
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            Debug.Log("Begin Drag");
            dragItem = Instantiate(dragItemPrefab, canvas.transform);
            SetDragItemPosition(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragItem != null)
            {
                SetDragItemPosition(eventData);
                Debug.Log("Dragging");
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Debug.Log("End Drag");
            if (dragItem != null)
                Destroy(dragItem);
        }

        private void SetDragItemPosition(PointerEventData eventData)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.GetComponent<RectTransform>(),  // The canvas RectTransform
                eventData.position,                    // Pointer position in screen space
                canvas.worldCamera,                    // The camera rendering the canvas (null for Screen Space - Overlay)
                out Vector3 worldPosition              // Output world space position
            );
            dragItem.transform.position = worldPosition;
        }

        public void OnDrop(PointerEventData eventData)
        {
            Debug.Log("Drop");
        }
        #endregion



    }
}