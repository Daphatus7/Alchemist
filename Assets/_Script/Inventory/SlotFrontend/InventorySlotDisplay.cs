using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Items;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _Script.Inventory.SlotFrontend
{
    public class InventorySlotDisplay : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button slotButton;
        [SerializeField] private Image highlight;
        [SerializeField] private GameObject dragItemPrefab;

        private static GameObject dragItem;
        private static Canvas canvas;

        private int _slotIndex;  
        public int SlotIndex => _slotIndex;

        private int _inventoryIndex; 
        public int InventoryIndex => _inventoryIndex;

        private SlotType _slotType; 
        public SlotType SlotType => _slotType;

        private IContainerUIHandle _inventoryUI;
        private ItemStack _currentStack;

        public string ItemTypeName => _currentStack?.IsEmpty == false ? _currentStack.ItemData.ItemName : "";
        public int Value => _currentStack?.IsEmpty == false ? _currentStack.ItemData.Value : 0;
        public int Quantity => _currentStack?.IsEmpty == false ? _currentStack.Quantity : 0;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex, SlotType slotType)
        {
            //ToDo: When the slot is initialized, it will move to the corresponding grid location
            
            /**
             *
             *
             *
             *
             * 
             */
            
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            _slotType = slotType;
            highlight.enabled = false;
        }

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex, int inventoryIndex, SlotType slotType)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            _inventoryIndex = inventoryIndex;
            _slotType = slotType;
            highlight.enabled = false;
        }

        private void OnEnable()
        {
            slotButton?.onClick.AddListener(OnSlotClicked);
        }

        private void OnDisable()
        {
            slotButton?.onClick.RemoveListener(OnSlotClicked);
        }

        /// <summary>
        /// Update slot display with the given ItemStack.
        /// </summary>
        public void SetSlot(ItemStack stack)
        {
            _currentStack = stack;
            if (_currentStack?.IsEmpty == false)
            {
                var data = _currentStack.ItemData;
                icon.enabled = true;
                icon.sprite = data.ItemSprite;
                icon.color = Color.white;
                quantityText.text = _currentStack.Quantity > 1 ? _currentStack.Quantity.ToString() : "";
            }
            else
            {
                ClearSlot();
            }
        }

        public virtual void ClearSlot()
        {
            _currentStack = null;
            icon.color = new Color(1, 1, 1, 0);
            icon.enabled = false;
            quantityText.text = "";
        }

        public virtual void OnSlotClicked()
        {
            Debug.Log("Slot clicked: " + _slotIndex);
            _inventoryUI.OnSlotClicked(this);
        }

        #region Highlight Methods
        public void HighlightSlot() => highlight.enabled = true;
        public void UnhighlightSlot() => highlight.enabled = false;
        #endregion

        #region Drag and Drop

        private bool CanDrag()
        {
            // Decide if dragging is allowed based on slot type
            switch (SlotType)
            {
                case SlotType.Merchant:
                case SlotType.ActionBar:
                case SlotType.PlayerInventory:
                case SlotType.Equipment:
                    return true;
                default:
                    return false;
            }
        }
        
        private bool CanDrop(InventorySlotDisplay sourceSlot)
        {
            // Determine if dropping into this slot is allowed
            switch (SlotType)
            {
                case SlotType.Merchant:
                    if (_inventoryUI is IMerchantHandler merchant)
                        return sourceSlot._currentStack?.IsEmpty == false && merchant.AcceptsItem(sourceSlot._currentStack);
                    return false;
                case SlotType.ActionBar:
                case SlotType.PlayerInventory:
                case SlotType.Equipment:
                    return true;
                default:
                    return false;
            }
        }
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!CanDrag() || _currentStack?.IsEmpty != false) return;

            icon.raycastTarget = false;
            if (dragItem == null)
                dragItem = Instantiate(dragItemPrefab, canvas.transform);
            else
                dragItem.SetActive(true);

            var dragItemImage = dragItem.GetComponent<Image>();
            if (dragItemImage != null)
            {
                dragItemImage.sprite = icon.sprite;
                dragItemImage.raycastTarget = false; 
            }

            SetDragItemPosition(eventData);
            icon.color = new Color(1, 1, 1, 0);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragItem != null)
                SetDragItemPosition(eventData);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!CanDrag()) return;
            if (dragItem != null)
            {
                dragItem.SetActive(false);
                icon.color = Color.white;
            }
            icon.raycastTarget = true;
        }

        public void OnDrop(PointerEventData eventData)
        {
            // Drag end, release in this slot
            var sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotDisplay>();
            if (sourceSlot?._currentStack?.IsEmpty != false)
                return;

            // Cache itemData for performance
            var itemData = sourceSlot._currentStack.ItemData;
            // Prevent placing a ContainerItem inside another Container (Bag)
            if (itemData is ContainerItem && _slotType == SlotType.Bag)
            {
                Debug.LogWarning("Cannot place a ContainerItem inside another Container (Bag).");
                return;
            }

            if (!CanDrop(sourceSlot)) return;

            // Determine drag type
            var dragType = GetDragType(sourceSlot);
            Debug.Log("Drag Type: " + dragType);

            switch (dragType)
            {
                case DragType.Swap:
                    SwapItems(sourceSlot);
                    break;
                case DragType.Buy:
                    if (sourceSlot._inventoryUI is IMerchantHandler merchant && _inventoryUI is IPlayerInventoryHandler player)
                    {
                        var purchasedItem = merchant.Purchase(player, sourceSlot._slotIndex);
                        //if the purchased item is not empty
                        if (purchasedItem?.IsEmpty == false)
                        {
                            //if the current slot is not empty
                            if (_currentStack?.IsEmpty == false)
                            {
                                Debug.Log("Current slot is not empty");
                                var remainingItem = _inventoryUI.AddItem(purchasedItem);

                                if (remainingItem?.IsEmpty == false)
                                {
                                    Debug.Log("Not enough space in inventory!");
                                }
                            }
                            else
                            {
                                _inventoryUI.AddItemToEmptySlot(purchasedItem, _slotIndex);
                            }
                        }
                    }
                    break;
                case DragType.Sell:
                    if (sourceSlot._inventoryUI is IPlayerInventoryHandler playerInv && _inventoryUI is IMerchantHandler merchantSelf)
                    {
                        if (merchantSelf.Sell(playerInv, sourceSlot))
                        {
                            _inventoryUI.AddItem(sourceSlot._currentStack);
                            sourceSlot._inventoryUI.RemoveAllItemsFromSlot(sourceSlot._slotIndex);
                        }
                        else
                        {
                            Debug.Log("Merchant does not accept the item");
                        }
                    }
                    break;
                default:
                    // Do nothing
                    break;
            }

            // Hide drag item visual
            if (dragItem != null)
                dragItem.SetActive(false);
        }

        private DragType GetDragType(InventorySlotDisplay sourceSlot)
        {
            // Determine drag type based on source and target SlotType
            switch (sourceSlot.SlotType)
            {
                case SlotType.Equipment:
                    switch (SlotType)
                    {
                        case SlotType.Equipment: return DragType.DoNothing;
                        case SlotType.PlayerInventory: return DragType.Buy;
                        case SlotType.ActionBar: return DragType.Sell;
                        case SlotType.Merchant: return DragType.DoNothing;
                        default: return DragType.DoNothing;
                    }
                case SlotType.PlayerInventory:
                    switch (SlotType)
                    {
                        case SlotType.Equipment: 
                        case SlotType.PlayerInventory:
                        case SlotType.ActionBar:
                            return DragType.Swap;
                        case SlotType.Merchant:
                            return DragType.Sell;
                        default:
                            return DragType.DoNothing;
                    }
                case SlotType.ActionBar:
                    switch (SlotType)
                    {
                        case SlotType.Equipment:
                        case SlotType.PlayerInventory:
                        case SlotType.ActionBar:
                            return DragType.Swap;
                        case SlotType.Merchant:
                            return DragType.Sell;
                        default:
                            return DragType.DoNothing;
                    }
                case SlotType.Merchant:
                    switch (SlotType)
                    {
                        case SlotType.PlayerInventory:
                            return DragType.Buy;
                        default:
                            return DragType.DoNothing;
                    }
                default:
                    return DragType.DoNothing;
            }
        }

        /// <summary>
        /// Swap items between slots.
        /// </summary>
        private void SwapItems(InventorySlotDisplay source)
        {
            //ToDo : Consider the case when where it is not possible to swap the items
      
            
            //1. same size - swap
            //2. different size - cannot swap
            //3. if the target slot is empty, calculate if can fit

            //if the current slot is empty, just add the item to the slot
            
            if (_currentStack == null || _currentStack.IsEmpty)
            {
                //the item cannot be fit int the slot, but it can be fit if the original item is removed
                
                if(_inventoryUI.CanFitItem(_slotIndex, source._currentStack))
                {
                    _inventoryUI.AddItemToEmptySlot(source._currentStack, _slotIndex);
                    source._inventoryUI.RemoveAllItemsFromSlot(source._slotIndex);
                }
            }
            else
            {
                //check if it has the same size
                if(_currentStack != null && _currentStack.ItemData.ItemShape.CompareShapes(source._currentStack.ItemData.ItemShape))
                {
                    var sourceItem = source._inventoryUI.RemoveAllItemsFromSlot(source._slotIndex);
                    var myItem = _inventoryUI.RemoveAllItemsFromSlot(_slotIndex);

                    //if the source has removed an item
                    if (sourceItem?.IsEmpty == false)
                        //add the item to the current slot
                        _inventoryUI.AddItemToEmptySlot(sourceItem, _slotIndex);

                    //if the current slot has removed an item
                    if (myItem?.IsEmpty == false)
                        //add the item to the source slot
                        source._inventoryUI.AddItemToEmptySlot(myItem, source._slotIndex);
                }
                else
                {
                    Debug.Log("Cannot swap items with different sizes.");
                }
            }
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

        #endregion
    }
    
    public static class SlotPool
    {
        private static readonly List<InventorySlotDisplay> _availableSlots = new List<InventorySlotDisplay>();
        private static GameObject _slotPrefab;
        private static Transform _poolParent;
        private static bool _initialized;

        public static void Initialize(GameObject slotPrefab, Transform poolParent, int initialCount)
        {
            if (_initialized) return;
            _initialized = true;
            _slotPrefab = slotPrefab;
            _poolParent = poolParent;

            // Pre-instantiate initialCount slots
            for (int i = 0; i < initialCount; i++)
            {
                var slot = CreateNewSlot();
                ReturnSlotToPool(slot);
            }
        }

        public static InventorySlotDisplay GetSlot()
        {
            if (_availableSlots.Count == 0)
            {
                // Pool is empty, create a new slot
                return CreateNewSlot();
            }

            int lastIndex = _availableSlots.Count - 1;
            var slot = _availableSlots[lastIndex];
            _availableSlots.RemoveAt(lastIndex);
            slot.gameObject.SetActive(true);
            return slot;
        }

        public static void ReturnSlotToPool(InventorySlotDisplay slot)
        {
            slot.gameObject.SetActive(false);
            slot.transform.SetParent(_poolParent);
            _availableSlots.Add(slot);
        }

        private static InventorySlotDisplay CreateNewSlot()
        {
            var slotObj = Object.Instantiate(_slotPrefab, _poolParent);
            slotObj.SetActive(false);
            return slotObj.GetComponent<InventorySlotDisplay>();
        }
    }

    public enum DragType
    {
        Swap,
        Add, // Not implemented yet
        Buy,
        Sell,
        DoNothing
    }

    // Keep SlotType and other interfaces consistent with the rest of the project
    public enum SlotType
    {
        PlayerInventory,
        Equipment,
        ActionBar,
        Merchant,
        Bag
    }
}