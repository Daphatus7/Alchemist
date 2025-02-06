using System;
using System.Collections.Generic;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
using _Script.Items;
using _Script.UserInterface;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _Script.Inventory.SlotFrontend
{
    public sealed class InventorySlotInteraction : MonoBehaviour, 
        IBeginDragHandler, 
        IDragHandler, 
        IEndDragHandler, 
        IDropHandler, 
        IPointerClickHandler,
        IPointerEnterHandler,
        IPointerExitHandler
    {
        [SerializeField] private GameObject dragItemPrefab;

        private static GameObject dragItem;
        private static Canvas canvas;

        [SerializeField] private bool isDebug;
        [SerializeField] private TextMeshProUGUI debugText;
        
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
            if (isDebug)
            {
                debugText.gameObject.SetActive(true);
            }
            else
            {
                debugText.gameObject.SetActive(false);
            }
        }

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex, SlotType slotType)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            _slotType = slotType;
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
            }
            else
            {
                ClearSlot();
            }

       
            // If we're debugging, show slot & item info in the debugText
            if (isDebug && debugText)
            {
                // Build a multi-line string showing relevant data
                string itemName = (_currentStack != null && !_currentStack.IsEmpty)
                    ? _currentStack.ItemData.ItemName
                    : "Empty";

                debugText.text =
                    $"<b></b> {itemName}\n" +
                    $"<b></b> {(_currentStack?.Quantity ?? 0)}";
            }
        }
        
        public void ClearSlot()
        {
            _currentStack = null;
        }

        public void HandleRightClick()
        {
            if (_isDragging)
            {
                return;
            }
            _inventoryUI.OnSlotClicked(this);
        }

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
            
            //******** icon.raycastTarget = false;
            if (dragItem == null)
                dragItem = Instantiate(dragItemPrefab, canvas.transform);
            else
                dragItem.SetActive(true);

            var dragItemImage = dragItem.GetComponent<Image>();
            
            if (dragItemImage != null)
            {
                //********  dragItemImage.sprite = icon.sprite;
                dragItemImage.raycastTarget = false; 
            }
            
            var removedItem = _inventoryUI.RemoveAllItemsFromSlot(_slotIndex);
                
            //Add the item to the dragItem
            DragItem.Instance.AddItemToDrag(removedItem, _inventoryUI.GetSlotPosition(_slotIndex));
            
            SetDragItemPosition(eventData);
           //******** icon.color = new Color(1, 1, 1, 0);
        }

        // private Vector2Int CalculatePivotPosition(ItemStack itemStack)
        // {
        //     //先得到物品的Pivot
        //     var itemPivot = itemStack.PivotPosition;
        //     //获取当前背包选中的位置
        //     var slotPosition = _inventoryUI.GetSlotPosition(_slotIndex);
        //     //计算出物品的偏移量
        //     
        //     return slotPosition - itemPivot;
        // }

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
                //******** icon.color = Color.white;
            }
            //******** icon.raycastTarget = true;
            
            // 2. 检查是否有有效的目标
            var dropTargetObj = eventData.pointerCurrentRaycast.gameObject;
            bool hasDropTarget = dropTargetObj != null && dropTargetObj.GetComponent<IDropHandler>() != null;
            
            if (!hasDropTarget)
            {
                ReturnItemToSourceSlot(this);
            }
            else if (dropTargetObj == gameObject)
            {
                OnDrop(eventData);
            }
        }
        
        
        /**
         * This triggers target slot's OnDrop
         */
        public void OnDrop(PointerEventData eventData)
        {
            var sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotInteraction>();
            
            if (sourceSlot == null) return;
            //Debug.Log("Dropped item on slot: " + DragItem.Instance.PeakItemStack());
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
                            //can afford the item
                            if (merchant.CanAfford(player, purchasedItem, purchasedItem.Quantity))
                            {
                                //if it can fit the item in the player inventory
                                var projectedPositions = DragItem.Instance.ProjectedPositions(_inventoryUI.GetSlotPosition(_slotIndex));
                                if (player.CanFitItem(projectedPositions))
                                {
                                    merchant.RemoveGold(player, purchasedItem, purchasedItem.Quantity);
                                    _inventoryUI.AddItemToEmptySlot(DragItem.Instance.RemoveItemStack(), projectedPositions);
                                    //Deduct the money from the player
                                }
                                //如果不能放进玩家的背包，那么应该返回到商人的背包
                                else
                                {
                                    DragItem.Instance.RemoveItemStack();
                                }
                                
                                /**
                                 * TODo: did not consider the case when the player can't fit the item in the inventory
                                 */
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
                case DragType.Add:
                    break;
                case DragType.DoNothing:
                    ReturnItemToSourceSlot(sourceSlot);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Hide drag item visual
            if (dragItem != null)
                dragItem.SetActive(false);
            
        }
        
        private void ReturnItemToSourceSlot(InventorySlotInteraction sourceSlot)
        {
            if (DragItem.Instance.PeakItemStack() == null)
            {
                return;
            }
            
            var itemToAdd = dragItem.GetComponent<DragItem>().RemoveItemStackOnFail();
            
            sourceSlot._inventoryUI.AddItemToEmptySlot(itemToAdd, itemToAdd.ItemPositions);
        }

        private DragType GetDragType(InventorySlotInteraction sourceSlot)
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
        private void SwapItems(InventorySlotInteraction source)
        {
            
            var targetSlotPosition = _inventoryUI.GetSlotPosition(_slotIndex);
            
            //先检查是不是在同一个背包里
            if (source.SlotType == SlotType)
            {
                //Debug.Log("Shifted Pivot Index: " + shiftedPivotIndex);
                var projectedPositions = DragItem.Instance.ProjectedPositions(targetSlotPosition);
                if(_inventoryUI.CanFitItem(projectedPositions))
                {
                    var itemToAdd = DragItem.Instance.RemoveItemStack();
                    _inventoryUI.AddItemToEmptySlot(itemToAdd, projectedPositions);
                }
                else
                {
                    Debug.Log("Can't fit the item");
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

        public void OnPointerEnter(PointerEventData eventData)
        {
            ItemDetail.Instance.ShowUI();
            if (_currentStack != null && !_currentStack.IsEmpty)
            {
                ItemDetail.Instance.ShowItemDetail(_currentStack.ItemData);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ItemDetail.Instance.HideUI();
        }
    }
    
    public static class SlotPool
    {
        private static readonly List<InventorySlotInteraction> _availableSlots = new List<InventorySlotInteraction>();
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

        public static InventorySlotInteraction GetSlot()
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

        public static void ReturnSlotToPool(InventorySlotInteraction slot)
        {
            slot.gameObject.SetActive(false);
            slot.transform.SetParent(_poolParent);
            _availableSlots.Add(slot);
        }

        private static InventorySlotInteraction CreateNewSlot()
        {
            var slotObj = Object.Instantiate(_slotPrefab, _poolParent);
            slotObj.SetActive(false);
            return slotObj.GetComponent<InventorySlotInteraction>();
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
        Bag,
        Cauldron
    }
}