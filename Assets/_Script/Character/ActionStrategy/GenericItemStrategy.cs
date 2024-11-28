using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Character.ActionStrategy
{
    public sealed class GenericStrategy : MonoBehaviour, IActionStrategy
    {
        /**
         * Consider this as temporary solution
         */
        [SerializeField] private GameObject itemSlot;
        private GameObject currentItem;
        
        public void LeftMouseButtonDown(Vector3 direction)
        {
            Debug.Log("Left Mouse Button Down");
        }

        public void LeftMouseButtonUp(Vector3 direction)
        {
            Debug.Log("Left Mouse Button Up");
        }
        
        public void ChangeItem(GameObject itemPrefab, ItemData itemData)
        {
            // Spawn item
            var item = Instantiate(itemPrefab, itemSlot.transform.position, Quaternion.identity);
            item.transform.parent = itemSlot.transform;
        }
        
        public void RemoveItem()
        {
            if(currentItem != null)
            {
                Destroy(currentItem.gameObject);
            }
        }
    }

}