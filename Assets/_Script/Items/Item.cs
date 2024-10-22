using UnityEngine;

namespace _Script.Items
{
    [System.Serializable]
    public class Item
    {
        public string itemName;
        public int itemID;
        public string itemDescription;
        public Sprite itemIcon;
        public int itemQuantity;
        public bool isStackable;

        public Item(string name, int id, string description, Sprite icon, int quantity, bool stackable)
        {
            itemName = name;
            itemID = id;
            itemDescription = description;
            itemIcon = icon;
            itemQuantity = quantity;
            isStackable = stackable;
        }

        public void UseItem()
        {
            Debug.Log("Using item: " + itemName);
            // Add logic for using the item (like healing or equipping)
        }
    }

}