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
    public class InventorySlotDisplay : MonoBehaviour, 
        IBeginDragHandler, 
        IDragHandler, 
        IEndDragHandler, 
        IDropHandler, 
        IPointerClickHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI quantityText;
        [SerializeField] private Button slotButton;
        [SerializeField] private Image highlight;
        [SerializeField] private GameObject dragItemPrefab;

        private static GameObject dragItem;
        private static Canvas canvas;

        private int _slotIndex; public int SlotIndex => _slotIndex;


        private SlotType _slotType; 
        public SlotType SlotType => _slotType;

        private IContainerUIHandle _inventoryUI;
        [SerializeField] private ItemStack _currentStack;
        
        public string ItemTypeName => _currentStack?.IsEmpty == false ? _currentStack.ItemData.ItemName : "";
        public int Value => _currentStack?.IsEmpty == false ? _currentStack.ItemData.Value : 0;
        public int Quantity => _currentStack?.IsEmpty == false ? _currentStack.Quantity : 0;

        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex, SlotType slotType)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            _slotType = slotType;
            highlight.enabled = false;
        }

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex, int inventoryIndex, SlotType slotType)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            _slotType = slotType;
            highlight.enabled = false;
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                HandleRightClick();
            }
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

        public virtual void HandleRightClick()
        {
            if (_isDragging)
            {
                return;
            }
    
            Debug.Log("Slot clicked: " + _slotIndex);
            _inventoryUI.OnSlotClicked(this);
        }

        #region Highlight Methods
        public void HighlightSlot() => highlight.enabled = true;
        public void UnhighlightSlot() => highlight.enabled = false;
        #endregion

        #region Drag and Drop

        private bool _isDragging;
        
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
        
        private bool CanDrop()
        {
            // Determine if dropping into this slot is allowed
            switch (SlotType)
            {
                case SlotType.Merchant:
                    if (_inventoryUI is IMerchantHandler merchant)
                    {
                        return !DragItem.Instance.PeakItemStack().IsEmpty && merchant.AcceptsItem(DragItem.Instance.PeakItemStack());
                    }
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
            _isDragging = true;
            
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
            
            var removedItem = _inventoryUI.RemoveAllItemsFromSlot(_slotIndex);
                
            //Add the item to the dragItem
            DragItem.Instance.AddItemToDrag(removedItem);
            
            SetDragItemPosition(eventData);
            icon.color = new Color(1, 1, 1, 0);
        }

        public void OnDrag(PointerEventData eventData)
        {
            //DragItem Should Be Singleton in this case or can be accssed through Service Locator
            if (dragItem != null)
            {
                //Remove item from the slot
                SetDragItemPosition(eventData);
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _isDragging = false;
            // 1. 做最小化的 UI 收尾
            if (dragItem != null)
            {
                dragItem.SetActive(false);
                icon.color = Color.white;
            }
            icon.raycastTarget = true;
            
            // 2. 检查是否有有效的目标
            var dropTargetObj = eventData.pointerCurrentRaycast.gameObject;
            bool hasDropTarget = dropTargetObj != null 
                                 && dropTargetObj.GetComponent<IDropHandler>() != null;
            
            if (!hasDropTarget)
            {
                Debug.Log("No valid drop target found, returning item to source slot.");
                ReturnItemToSourceSlot(this);
            }

            // Let OnDrop handle actual item placement logic if a valid target is found
        }

        public void OnDrop(PointerEventData eventData)
        {
            
            var sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotDisplay>();
            
            if (sourceSlot == null) return;
            
            var itemData = DragItem.Instance.PeakItemStack()?.ItemData;
            if(itemData == null) return;
            
            // Prevent placing a ContainerItem inside another Container (Bag)
            if (itemData is ContainerItem && _slotType == SlotType.Bag)
            {
                Debug.Log("Cannot place a ContainerItem inside another Container (Bag).");
                ReturnItemToSourceSlot(sourceSlot);
                return;
            }
            
            if (!CanDrop())
            {
                //Return the item to where it was
                Debug.Log("Cannot drop item here, now returning to source slot");
                ReturnItemToSourceSlot(sourceSlot);
                return;
            }

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
                        var purchasedItem = DragItem.Instance.PeakItemStack();
                        //if the purchased item is not empty
                        if (purchasedItem?.IsEmpty == false)
                        {
                            if (merchant.Purchase(player, purchasedItem, purchasedItem.Quantity))
                            {
                                //if can fit the item in the player inventory
                                if (player.CanFitItem(_slotIndex, purchasedItem))
                                {
                                    _inventoryUI.AddItemToEmptySlot(DragItem.Instance.RemoveItemStack(), _slotIndex);
                                }
                                else
                                {
                                    _inventoryUI.AddItem(DragItem.Instance.RemoveItemStack());
                                }
                            }
                            else
                            {
                                Debug.Log("You have no money");
                            }

                        }
                        else
                        {
                            throw new Exception("Purchased item is empty");
                        }
                    }
                    break;
                case DragType.Sell:
                    Debug.Log("Sell item");
                    if (sourceSlot._inventoryUI is IPlayerInventoryHandler playerInv && _inventoryUI is IMerchantHandler merchantSelf)
                    {
                        if (merchantSelf.Sell(playerInv, DragItem.Instance.PeakItemStack()))
                        {
                            _inventoryUI.AddItem(DragItem.Instance.RemoveItemStack());
                        }
                        else
                        {
                            Debug.Log("Merchant does not accept the item");
                        }
                    }
                    break;
            }

            // Hide drag item visual
            if (dragItem != null)
                dragItem.SetActive(false);
        }
        
        
        private void ReturnItemToSourceSlot(InventorySlotDisplay sourceSlot)
        {
            if (DragItem.Instance.PeakItemStack() == null) return;
            int pivotIndex = sourceSlot._inventoryUI.GetSlotIndex(DragItem.Instance.PeakItemStack().PivotPosition);
            sourceSlot._inventoryUI.AddItemToEmptySlot(dragItem.GetComponent<DragItem>().RemoveItemStack(), pivotIndex);
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
            
            //目前假设两个物品在同一个背包里
            //
            var pivot = DragItem.Instance.PeakItemStack().PivotPosition;
            var shiftVector = _inventoryUI.GetSlotPosition(_slotIndex) - source._inventoryUI.GetSlotPosition(source._slotIndex);
            var shiftedPivot = shiftVector + pivot;
            var shiftedPivotIndex = _inventoryUI.GetSlotIndex(shiftedPivot);
            
            
            //先检查是不是在同一个背包里
            if (source.SlotType == SlotType)
            {
                int count = _inventoryUI.GetItemsCount(shiftedPivotIndex, DragItem.Instance.PeakItemStack().ItemData.ItemShape.Positions, out var onlyItemIndex);
                if(count == 0)
                {
                    if(_inventoryUI.CanFitItem(shiftedPivotIndex, DragItem.Instance.PeakItemStack()))
                    {
                        _inventoryUI.AddItemToEmptySlot(DragItem.Instance.RemoveItemStack(), shiftedPivotIndex);
                    }
                    else
                    {
                        Debug.Log("Can't fit the item");
                        ReturnItemToSourceSlot(source);
                    }
                }
                else
                {
                    //把东西放回去
                    ReturnItemToSourceSlot(source);
                }
            }
            //如果不是 先报错
            else
            {
                throw new Exception("还没有应用跨背包交换物品的功能");
            }
            
            
            //先检查重叠多少物品
            
            // Index 是对的
            
            //但是
            
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