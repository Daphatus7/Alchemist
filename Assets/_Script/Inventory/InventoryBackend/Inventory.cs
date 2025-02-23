using System;
using System.Collections.Generic;
using _Script.Inventory.ItemInstance;
using _Script.Inventory.SlotFrontend;
using _Script.Items;
using _Script.Items.AbstractItemTypes._Script.Items;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

namespace _Script.Inventory.InventoryBackend
{
    public abstract class Inventory: IInventorySaveDataHandler
    {
        private readonly int _height; public int Height => _height;
        private readonly int _width; public int Width => _width;

        public int Capacity => _height * _width;
        private string _uniqueID; public string UniqueID
        {
            get
            {
                if(string.IsNullOrEmpty(_uniqueID))
                {
                    _uniqueID = Guid.NewGuid().ToString();
                }
                return _uniqueID;
            }
            set => _uniqueID = value;
        }

        /**
         * The shape-based slots in the inventory.
         * Each slot is an InventorySlot that may or may not contain an ItemInstance.
         */
        protected readonly InventorySlot[] Slots;
        
        public int SlotCount => Slots.Length;
        
        public ItemInstance.ItemInstance GetItemInstanceAt(int index)
        {
            if (index < 0 || index >= SlotCount)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }
            if(Slots[index] == null)
            {
                Debug.LogWarning("Invalid slot index.");
                return null;
            }
            return Slots[index] == null ? null : Slots[index].ItemInstance;
        }
        private readonly List<ItemInstance.ItemInstance> _itemInstances; public List<ItemInstance.ItemInstance> ItemInstances => _itemInstances;

        // ----------------------------------------------
        // Constructors
        // ----------------------------------------------

        /// <summary>
        /// Load an empty inventory with given height, width.
        /// </summary>
        public Inventory(int height, int width)
        {
            _height = height;
            _width = width;
            Slots = new InventorySlot[Capacity];
            _itemInstances = new List<ItemInstance.ItemInstance>();

            // Initialize slots
            for (int i = 0; i < Capacity; i++)
            {
                Slots[i] = new InventorySlot();
            }
        }

        /// <summary>
        /// Load an inventory with items (restoring from save, etc.)
        /// </summary>
        public Inventory(int height, int width, ItemInstance.ItemInstance[] ItemInstance)
        {
            _height = height;
            _width = width;
            
            Slots = new InventorySlot[Capacity];
            _itemInstances = new List<ItemInstance.ItemInstance>();

            // Initialize slots
            for (int i = 0; i < Capacity; i++)
            {
                Slots[i] = new InventorySlot();
            }
            
            // Load items
            foreach (var item in ItemInstance)
            {
                AddItem(item);
            }
        }

        // ----------------------------------------------
        // Abstract properties / events
        // ----------------------------------------------
        public abstract SlotType SlotType { get; }

        // Event: inventory changed at a slot index
        public event Action<int> OnInventorySlotChanged;
        
        public event Action onItemInstanceChanged;

        // Helper method to invoke the event
        public void OnInventorySlotChangedEvent(List<Vector2Int> positions)
        {
            foreach (var pos in positions)
            {
                OnInventorySlotChanged?.Invoke(GridToSlotIndex(pos.x, pos.y));
            }
        }

        // ----------------------------------------------
        // Public API: AddItem
        // ----------------------------------------------

        /// <summary>
        /// Attempts to add an item instance to the inventory:
        /// 1. Merge into existing instances first (partial instance logic).
        /// 2. If not fully merged, try placing into empty slots via shape-based logic.
        /// Returns null if fully placed, or the leftover if not enough space.
        /// </summary>
        public ItemInstance.ItemInstance AddItem(ItemInstance.ItemInstance itemInstanceToAdd)
        {
            if (itemInstanceToAdd == null)
            {
                return null;
            }

            // 1) Merge with existing partial instances
            foreach(var existingItemInstance in _itemInstances)
            {
                if (existingItemInstance != null &&
                    existingItemInstance.Equals(itemInstanceToAdd) &&
                    existingItemInstance.Quantity < existingItemInstance.MaxStackSize)
                {
                    int oldQuantity = itemInstanceToAdd.Quantity;
                    int remaining   = existingItemInstance.TryAdd(itemInstanceToAdd);
                    itemInstanceToAdd.Quantity = remaining;

                    //if some merging happened
                    if (remaining < oldQuantity)
                    {
                        // Some merging happened
                        OnInventorySlotChangedEvent(existingItemInstance.ItemPositions);
                        
                        int mergedQuantity = oldQuantity - remaining;
                        
                        // Update the inventory status
                        InventoryStatus.UpdateInventoryStatus(existingItemInstance.ItemID, mergedQuantity);
                    }
                    
                    if (remaining == 0)
                    {
                        // Fully merged, return null because there's no leftover
                        return null;
                    }
                }            
            }
            
            // 2) Shape-based placement for leftover
            for (var i = 0; i < Capacity; i++)
            {
                //checking if the item can fit in the inventory
                var projectedPositions = itemInstanceToAdd.ItemShape.ProjectedPositions(SlotIndexToGrid(i));
                if (CanFitIn(projectedPositions))
                {
                    // Decide how many to place
                    int toAdd = Mathf.Min(itemInstanceToAdd.Quantity, itemInstanceToAdd.MaxStackSize);

                    // Actually place the item
                    var placedInstance = CreateItemInstanceAtLocation(projectedPositions, itemInstanceToAdd, toAdd);
                    // CreateItemInstanceAtLocation(projectedPositions, toAdd, itemInstanceToAdd);
                    if (placedInstance != null)
                    {
                        // Adjust leftover
                        _itemInstances.Add(placedInstance);
                        itemInstanceToAdd.Quantity -= toAdd;
                        
                        // Notify that the item instances have changed.
                        OnItemInstanceChanged();
                        InventoryStatus.UpdateInventoryStatus(placedInstance.ItemID, toAdd);

                        if (itemInstanceToAdd.Quantity <= 0)
                        {
                            // Done
                            return null;
                        }
                    }
                    // If placement failed, continue searching
                }
            }

            // 3) Not enough space
            return itemInstanceToAdd;
        }
        
        /// <summary>
        /// Allocate the item instance by copying the original item instance and assigning the quantity.
        /// </summary>
        /// <param name="projectedPositions"></param>
        /// <param name="itemInstance"></param>
        /// <param name="quantity"></param>
        /// <returns></returns>
        private ItemInstance.ItemInstance CreateItemInstanceAtLocation(List<Vector2Int> projectedPositions, 
            ItemInstance.ItemInstance itemInstance,
            int quantity)
        {
            if(projectedPositions == null || projectedPositions.Count == 0)
            {
                Debug.LogWarning("Invalid item instance positions.");
                return null;
            }

            //Copy the item instance
            var placedInstance = itemInstance.Clone();
            placedInstance.Quantity = quantity;
            placedInstance.ItemPositions = projectedPositions;
            foreach (var slotPos in projectedPositions)
            {
                var sIndex = GridToSlotIndex(slotPos.x, slotPos.y);
                Slots[sIndex].ItemInstance = placedInstance;
            }
            OnInventorySlotChangedEvent(projectedPositions);
            return placedInstance;
        }
        
        // ----------------------------------------------
        // Checking shape fit
        // ----------------------------------------------

        /// <summary>
        /// Checks if the specified shape can fit, with top-left corner at 'pos' (in grid coords),
        /// by verifying every offset is in range and the slot is empty.
        /// </summary>
        private bool CanFitIn(List<Vector2Int> projectedPositions)
        {
            if(projectedPositions == null || projectedPositions.Count == 0)
            {
                Debug.LogError("Invalid item projectedPositions to compare.");
                return false;
            }
            
            foreach (var pos in projectedPositions)
            {
                int gx = pos.x;
                int gy = pos.y;

                // Check bounds
                if (gx < 0 || gx >= _width || gy < 0 || gy >= _height)
                {
                    return false;
                }

                int finalIndex = GridToSlotIndex(gx, gy);
                if (!Slots[finalIndex].IsEmpty)
                {
                    return false;
                }
            }
            
            return true;
        }


        public bool CanFitItem(List<Vector2Int> projectedPositions)
        {
            
            if (projectedPositions == null || projectedPositions.Count == 0)
            {
                Debug.Log("Invalid item stack to compare.");
                return false;
            }
            
//            Debug.Log("Cp " + SlotIndexToGrid(mySlot));
            
            return CanFitIn(projectedPositions);    
        }
        
        // ----------------------------------------------
        // Remove entire stack from a slot
        // ----------------------------------------------
        public ItemInstance.ItemInstance RemoveAllItemsFromSlot(int slotIndex)
        {
            //invalid slot index
            if (slotIndex < 0 || slotIndex >= Capacity) return null;
            
            var itemInstance = GetItemInstanceAt(slotIndex);
            if (itemInstance == null)
            {
                return null;
            }
            
            //Create split all instances into a different stack with no assigned addresses in the inventory
            ClearItemInstance(itemInstance);
            return itemInstance;
        }
        
        /// <summary>
        ///  * 1. Clear the item connections of the stack item
        ///  * 2. Clear the item instance
        /// </summary>
        /// <param name="slotIndex"></param>
        private void OnRemovingItem(int slotIndex)
        {
            var itemInstance = GetItemInstanceAt(slotIndex);
            ClearItemInstance(itemInstance);
        }
        
        /// <summary>
        /// Remove the trace of the item instance from the inventory
        /// </summary>
        /// <param name="itemInstance"></param>
        private void ClearItemInstance(ItemInstance.ItemInstance itemInstance)
        {
            if(itemInstance == null) return;  // Remove the check for IsEmpty
            InventoryStatus.UpdateInventoryStatus(itemInstance.ItemID, -itemInstance.Quantity);
            foreach(var pos in itemInstance.ItemPositions)
            {
                var sIndex = GridToSlotIndex(pos.x, pos.y);
                Slots[sIndex].Clear();
            }
            OnInventorySlotChangedEvent(itemInstance.ItemPositions);
            _itemInstances.Remove(itemInstance);
            OnItemInstanceChanged();
        }

        public void AddItemToEmptySlot(ItemInstance.ItemInstance itemInstance // the original item instance need to be set to null
            , List<Vector2Int> projectedPositions)
        {
            //create a new item stack 
            var placedInstance = CreateItemInstanceAtLocation(projectedPositions, itemInstance, itemInstance.Quantity);
            _itemInstances.Add(placedInstance);
            OnItemInstanceChanged();
            OnInventorySlotChangedEvent(projectedPositions);
            InventoryStatus.UpdateInventoryStatus(placedInstance.ItemID, placedInstance.Quantity);
        }
        
        // ----------------------------------------------
        // Remove item from the inventory by quantity
        // ----------------------------------------------
        public virtual bool RemoveItemById(string itemId, int quantity = 1)
        {
            // Validate input
            if (string.IsNullOrEmpty(itemId))
            {
                Debug.LogWarning("Invalid itemId provided.");
                return false;
            }
            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove: " + quantity);
                return false;
            }

            // First, compute the total available amount for the specified itemId.
            int totalAvailable = 0;
            foreach (var stack in _itemInstances)
            {
                if (stack != null && stack.ItemID.Equals(itemId))
                {
                    totalAvailable += stack.Quantity;
                }
            }

            // If there aren’t enough items available, then nothing will be removed.
            if (totalAvailable < quantity)
            {
                Debug.Log($"Not enough items with id {itemId} to remove. Requested: {quantity}, available: {totalAvailable}");
                return false;
            }

            // Remove the required quantity from stacks.
            int remaining = quantity;
            // Iterate over a copy so that if a whole stack is removed (which clears it from _ItemInstances)
            // the iteration is not affected.
            var itemInstancesCopy = new List<ItemInstance.ItemInstance>(_itemInstances);
            foreach (var instance in itemInstancesCopy)
            {
                if (remaining <= 0)
                    break;

                if (instance != null && instance.ItemID.Equals(itemId))
                {
                    if (instance.Quantity <= remaining)
                    {
                        // If this stack has less than or equal to the remaining quantity,
                        // remove the whole stack.
                        remaining -= instance.Quantity;
                        // Pick one of the positions to trigger the removal. OnRemovingItem will clear
                        // all slots associated with the item and update InventoryStatus.
                        int slotIndex = GridToSlotIndex(instance.ItemPositions[0].x, instance.ItemPositions[0].y);
                        OnRemovingItem(slotIndex);
                    }
                    else
                    {
                        // If the current instance has more than we need, just subtract the required amount.
                        instance.Quantity -= remaining;
                        InventoryStatus.UpdateInventoryStatus(itemId, -remaining);
                        OnInventorySlotChangedEvent(instance.ItemPositions);
                        // Notify that the item instances have changed.
                        OnItemInstanceChanged();
                        remaining = 0;
                    }
                }
            }
            return true;
        }

        
        
        // ----------------------------------------------
        // Remove item from a specific slot
        // ----------------------------------------------
        protected bool RemoveItemFromSlot(int slotIndex, int quantity)
        {
            if (slotIndex < 0 || slotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return false;
            }

            var itemInstance = GetItemInstanceAt(slotIndex);

            if (quantity <= 0)
            {
                Debug.LogWarning("Invalid quantity to remove.");
                return false;
            }

            if (itemInstance.Quantity >= quantity)
            {
                itemInstance.Quantity -= quantity;

                InventoryStatus.UpdateInventoryStatus(itemInstance.ItemID, -quantity);

                var itemPositions = itemInstance.ItemPositions;
                if (itemInstance.Quantity <= 0)
                {
                    OnItemUsedUp(slotIndex);
                    OnRemovingItem(slotIndex);
                }
                OnInventorySlotChangedEvent(itemPositions);
                return true;
            }
            else
            {
                Debug.Log("Not enough items in slot to remove.");
                return false;
            }
        }
        
        protected virtual void OnItemUsedUp(int slotIndex)
        {
            Debug.Log("Item used up.");
        }

        public virtual void LeftClickItem(int slotIndex)
        {
            
        }
        
        public int GetItemsCountAtPositions(int pivotIndex
            , List<Vector2Int> projectedPositions, out int overlapIndex)
        {
            overlapIndex = -1;

            if (pivotIndex < 0 || pivotIndex >= Capacity)
            {
                Debug.LogWarning("Invalid slot index.");
                return 0;
            }
            
            var itemPivot = SlotIndexToGrid(pivotIndex);
            
            var foundItem = new Dictionary<ItemInstance.ItemInstance, int>();
            
            // check each slot by adding the offset to the pivot
            foreach(var offset in projectedPositions)
            {
                var offsetPos = itemPivot + offset;
                var sIndex = GridToSlotIndex(offsetPos.x, offsetPos.y);
                if(sIndex < 0 || sIndex >= Capacity)
                {
                    return Int32.MaxValue;
                }
                var slot = Slots[sIndex];
                if (!slot.IsEmpty)
                {
                    overlapIndex = sIndex; //Any overlap index, but only if the count is 1 will be considered
                    if (!foundItem.TryAdd(Slots[sIndex].ItemInstance, 1))
                    {
                        foundItem[Slots[sIndex].ItemInstance]++;
                    }
                }
            }
            return foundItem.Count;
        }
        
        
        
        
        
        
        
        
        // ----------------------------------------------
        // Coord helpers
        // ----------------------------------------------
        public int GridToSlotIndex(int x, int y)
        {
            return x * _height + y;
        }

        public Vector2Int SlotIndexToGrid(int slotIndex)
        {
            int gx = slotIndex / _height;
            int gy = slotIndex % _height;
            return new Vector2Int(gx, gy);
        }

        private bool IsInRange(int x, int y)
        {
            return (x >= 0 && x < _width && y >= 0 && y < _height);
        }

        private void OnItemInstanceChanged()
        {
            onItemInstanceChanged?.Invoke();
        }

        #region Inventory Status
        private InventoryStatus _inventoryStatus;
        public InventoryStatus InventoryStatus => _inventoryStatus ??= new InventoryStatus();
        
        /// <summary>
        /// Simply check the item status instead of going through the inventory.
        /// </summary>
        /// <param name="itemID"></param>
        /// <returns></returns>
        public int GetItemCount(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                Debug.LogWarning("Invalid itemID provided.");
                return -1;
            }
            InventoryStatus.GetStatus.TryGetValue(itemID, out int count);
            return count;
        }
        
        /// <summary>
        /// Get Realtime Item Count
        /// </summary>
        /// <param name="itemID"></param>
        /// <param name="requiredQuantity"></param>
        /// <returns></returns>
        public bool CheckRealtimeItemCount(string itemID, int requiredQuantity)
        {
            if (string.IsNullOrEmpty(itemID))
            {                
                Debug.LogWarning("Invalid itemID provided.");
                return false;
            }
            
            if (requiredQuantity <= 0)
            {
                Debug.LogWarning("Invalid requiredQuantity provided.");
                return false;
            }
            var count = 0;
            foreach (var itemInstance in _itemInstances)
            {
                if(itemInstance.ItemID.Equals(itemID))
                {
                    count += itemInstance.Quantity;
                    // If we have enough, return true
                    if(count >= requiredQuantity)
                    {
                        return true;
                    }
                }
            }
            Debug.Log("Not enough items with id " + itemID + " to remove. Requested: " + requiredQuantity + ", available: " + count);
            // after checking entire still not enough
            return false;
        }

        public bool IsEmpty
        {
            get
            {
                Debug.Log("Checking if empty + could be bugs where the instance was not removed correctly");
                return _itemInstances.Count == 0;
            }
        }
        
        public void SubscribeToInventoryStatus(Action<string, int> action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }
            InventoryStatus.OnInventoryStatusChanged += action;
        }
        
        public void UnsubscribeToInventoryStatus(Action<string, int> action)
        {
            InventoryStatus.OnInventoryStatusChanged -= action;
        }

        #endregion

        #region Save and Load
        
        public InventorySave OnSaveData()
        {
            var save = new InventorySave
            {
                inventoryUniqueID = UniqueID,
                items = new ItemSave[_itemInstances.Count],
                height = _height,
                width = _width
            };
            
            foreach(var instance in _itemInstances)
            {
                if(instance == null)
                {
                    throw new ArgumentNullException(nameof(instance) + " is null.");
                }
                var itemSave = instance.OnSaveData();
                save.items[_itemInstances.IndexOf(instance)] = itemSave;
            }
            
            return save;
        }

        /// <summary>
        /// Called after the inventory is created to load the data.
        /// </summary>
        /// <param name="saves"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual void OnLoadData(ItemSave[] saves)
        {
            //make sure the save is valid
            if (saves == null)
            {
                throw new ArgumentNullException(nameof(saves) + " is null.");
            }
            //Check every item
            foreach (var itemSave in saves)
            {
                if (itemSave != null && itemSave.ItemID != null)
                {
                    //recreate the item instance
                    
                    var newInstance = ItemInstanceFactory.RecreateItemInstanceSave(itemSave);
                    
                    //initialize the item instance to match the save
                    itemSave.InitializeItem(newInstance);
                    foreach (var pos in newInstance.ItemPositions)
                    {
                        var sIndex = GridToSlotIndex(pos.x, pos.y);
                        Slots[sIndex].ItemInstance = newInstance;
                    }
                }
            }
        }
        #endregion
    }
       
    /// <summary>
    /// string : item id
    /// int : quantity
    /// </summary>
    public sealed class InventoryStatus
    {
        private Dictionary<string, int> Status { get; } = new Dictionary<string, int>();
        public Dictionary<string, int> GetStatus => Status;
        
        public event Action<string, int> OnInventoryStatusChanged;

        internal void UpdateInventoryStatus(string itemID, int quantityChange)
        {
            // If adding a new itemID that doesn't exist yet
            if (!Status.ContainsKey(itemID) && quantityChange > 0)
            {
                Status[itemID] = quantityChange;
                OnOnInventoryStatusChanged(itemID, quantityChange);
                return;
            }

            // If updating an existing itemID
            if (Status.ContainsKey(itemID))
            {
                Status[itemID] += quantityChange;

                // Remove if it goes to zero or negative
                if (Status[itemID] <= 0)
                {
                    Status.Remove(itemID);
                    OnOnInventoryStatusChanged(itemID, 0); // Send 0 explicitly instead of Status[itemID]
                    return; // Early return to prevent invalid Status[itemID] lookup
                }
            }

            // Send update only if the item still exists
            if (Status.ContainsKey(itemID))
            {
                OnOnInventoryStatusChanged(itemID, Status[itemID]);
            }
            else
            {
                OnOnInventoryStatusChanged(itemID, 0); // Ensure zero is reported if removed
            }
        }

        private void PrintStatus()
        {
            Debug.Log("Current Inventory Status: ");
            foreach (var pair in Status)
            {
                Debug.Log(pair.Key + " : " + pair.Value);
            }
            Debug.Log("----------End of Inventory Status----------");
        }

        private void OnOnInventoryStatusChanged(string itemID, int count)
        {
            OnInventoryStatusChanged?.Invoke(itemID, count);
        }
    }
    
    [Serializable]
    public class InventorySave
    {
        public string inventoryUniqueID;
        public ItemSave [] items;
        public int height;
        public int width;
    }

    public interface IInventorySaveDataHandler
    {
        InventorySave OnSaveData();
        string UniqueID { get; }
        void OnLoadData(ItemSave [] save);
    }
}