using _Script.Inventory.InventoryBackend;
using _Script.Inventory.InventoryFrontendHandler;
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

        public string ItemTypeName => currentItem.ItemData.ItemName;
        public int Value => currentItem.ItemData.Value;
        public int Quantity => currentItem.Quantity;
        
        private SlotType _slotType; public SlotType SlotType => _slotType;
        
        private IContainerUIHandle _inventoryUI;
        private int _slotIndex;  public int SlotIndex => _slotIndex;

        /**
         * only visual representation of the item
         */
        private InventoryItem currentItem;
        
        private void Start()
        {
            canvas = GetComponentInParent<Canvas>();
        }

        public void InitializeInventorySlot(IContainerUIHandle inventoryUI, int slotIndex, SlotType slotType)
        {
            _inventoryUI = inventoryUI;
            _slotIndex = slotIndex;
            
            //hide
            highlight.enabled = false;
            _slotType = slotType;
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


        
        public void HighlightSlot()
        {
            highlight.enabled = true;
        }
        
        public void UnhighlightSlot()
        {
            highlight.enabled = false;
        }


        private bool CanDrag()
        {
            if(SlotType == SlotType.Merchant)
            {
                Debug.Log("Currently Set to true");
                return true;
            }
            else if (SlotType == SlotType.ActionBar)
            {
                return true;
            }
            else if (SlotType == SlotType.PlayerInventory)
            {
                return true;
            }
            else if (SlotType == SlotType.Equipment)
            {
                return true;
            }
            return false;
        }
        
        private bool CanDrop(InventorySlotDisplay sourceSlot)
        {
            switch (SlotType)
            {
                case SlotType.Merchant:
                    if(_inventoryUI is IMerchantHandler merchant)
                    {
                        return merchant.AcceptsItem(sourceSlot.currentItem);
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
            if(!CanDrag()) return;
            //If item is not null, then start dragging
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
            if(!CanDrag()) return;
            if (dragItem != null)
            {
                dragItem.SetActive(false);
                icon.color = Color.white;
            }
            icon.raycastTarget = true;
        }


        public void OnDrop(PointerEventData eventData)
        {

            //Check if the request is valid
            
            //Get the source slot, which is the request comes from
            InventorySlotDisplay sourceSlot = eventData.pointerDrag?.GetComponent<InventorySlotDisplay>();
            

            //if the source slot is empty then return
            if (sourceSlot == null || sourceSlot.currentItem == null)
            {
                return;
            }
            
            if(!CanDrop(sourceSlot)) return;
            
            //if something is coming from the source slot
            if (sourceSlot != this)
            {
                
                var dragType = GetDragType(sourceSlot);

                Debug.Log("Drag Type: " + dragType);
                switch (dragType)
                {
                    
                    case DragType.Swap:
                        SwapItems(sourceSlot);
                        break;
                    case DragType.Buy:
                        //See if the player can buy the item
                        if (sourceSlot._inventoryUI is IMerchantHandler merchant)
                        {
                            if (_inventoryUI is IPlayerInventoryHandler player)
                            {
                                //purchase the item
                                if (merchant.Purchase(player, sourceSlot._slotIndex) is { } item)
                                {
                                    //consider if there is an item in the target slot
                                    if(currentItem != null)
                                    {
                                        //add item to inventory
                                        var remainingItem = _inventoryUI.AddItem(item);
                                    }
                                    else
                                    {
                                        _inventoryUI.AddItemToEmptySlot(item, _slotIndex);
                                    }
                                    //consider adding item to inventory other than the player inventory
                                }
                            }
                        }
                        break;
                    case DragType.Sell:
                        //the item is being sent to the merchant
                        Debug.Log("sell" + sourceSlot._inventoryUI);
                        if (sourceSlot._inventoryUI is IPlayerInventoryHandler playerInventory)
                        {
                            if (_inventoryUI is IMerchantHandler merchantSelf)
                            {
                                if (merchantSelf.Sell(playerInventory, sourceSlot))
                                {
                                    _inventoryUI.AddItem(sourceSlot.currentItem);
                                    //remove the item from the player inventory
                                    sourceSlot._inventoryUI.RemoveAllItemsFromSlot(sourceSlot._slotIndex);
                                }
                                else
                                {
                                    Debug.Log("Merchant does not accept the item");
                                    //the merchant does not accept the trade
                                    //do nothing for now
                                }
                            }
                        }
                        
                        break;
                    case DragType.DoNothing:
                        break;
                    default:
                        break;
                }
                
                //first check if the item can be swapped
                /*
                 * Cases where the item cannot be swapped
                 * 1. if the target inventory is equipment inventory
                 *  a. if the source is in equipment inventory
                 *      i. they are both in the same equipment slot, check if the item type is the same
                 *      ii. they are in different equipment slot, then return fail
                 *  b. the source is not in other inventory, 
                 */
                
                

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
        
        private DragType GetDragType(InventorySlotDisplay sourceSlot)
        {
            switch (sourceSlot.SlotType)
            {
                //From Equipment to other slots
                case SlotType.Equipment:
                    switch (SlotType)
                    {
                        //From Equipment to Equipment
                        case SlotType.Equipment:
                            return DragType.DoNothing;
                        //From Equipment to Inventory
                        case SlotType.PlayerInventory:
                            return DragType.Buy;
                        //From Equipment to ActionBar
                        case SlotType.ActionBar:
                            return DragType.Sell;
                        //From Equipment to Merchant
                        case SlotType.Merchant:
                            return DragType.DoNothing;
                        default:
                            return DragType.DoNothing;
                    }
                //from player inventory to other slots
                case SlotType.PlayerInventory:
                    switch (SlotType)
                    {
                        //From player inventory to Equipment
                        case SlotType.Equipment:
                            return DragType.Swap;
                        //From player inventory to Inventory
                        case SlotType.PlayerInventory:
                            return DragType.Swap;
                        //From player inventory to ActionBar
                        case SlotType.ActionBar:
                            return DragType.Swap;
                        //From player inventory to Merchant
                        case SlotType.Merchant:
                            return DragType.Sell;
                        default:
                            return DragType.DoNothing;
                    }
                //From ActionBar to other slots
                case SlotType.ActionBar:
                    switch (SlotType)
                    {
                        //From ActionBar to Equipment
                        case SlotType.Equipment:
                            return DragType.Swap;
                        //From ActionBar to Inventory
                        case SlotType.PlayerInventory:
                            return DragType.Swap;
                        //From ActionBar to ActionBar
                        case SlotType.ActionBar:
                            return DragType.Swap;
                        //From ActionBar to Merchant
                        case SlotType.Merchant:
                            return DragType.Sell;
                        default:
                            return DragType.DoNothing;
                    }
                //From Merchant to other slots
                case SlotType.Merchant:
                    switch (SlotType)
                    {
                        //From Merchant to Equipment
                        case SlotType.Equipment:
                            return DragType.DoNothing;
                        //From Merchant to Inventory
                        case SlotType.PlayerInventory:
                            return DragType.Buy;
                        //From Merchant to ActionBar
                        case SlotType.ActionBar:
                            return DragType.DoNothing;
                        //From Merchant to Merchant
                        case SlotType.Merchant:
                            return DragType.DoNothing;
                        default:
                            return DragType.DoNothing;
                    }
                default:
                    return DragType.DoNothing;
            }
        }
        
        
        /// <summary>
        /// Swap the items or simply add to the target slot
        /// </summary>
        /// <param name="source"></param>
        private void SwapItems(InventorySlotDisplay source)
        {
            var sourceItem = source._inventoryUI.RemoveAllItemsFromSlot(source._slotIndex);
            var myItem = _inventoryUI.RemoveAllItemsFromSlot(_slotIndex);
                
            //consider the case where the source slot is empty
            if (sourceItem != null && sourceItem.ItemData != null)
            {
                _inventoryUI.AddItemToEmptySlot(sourceItem, _slotIndex);
            }
            if(myItem != null && myItem.ItemData != null)
            {
                source._inventoryUI.AddItemToEmptySlot(myItem, source._slotIndex);
            }
        }

        private InventoryItem RemoveInventoryItem(int slotIndex)
        {
            return _inventoryUI.RemoveAllItemsFromSlot(slotIndex);
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
    
    public enum SlotType
    {
        PlayerInventory,
        Equipment,
        ActionBar,
        Merchant,
    }

    public enum DragType
    {
        Swap,
        Add, //Currently not implemented 
        Buy,
        Sell,
        DoNothing
    }
}
