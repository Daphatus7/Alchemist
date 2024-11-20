using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class ItemLootable : MonoBehaviour
    {
        [SerializeField] private ItemData itemData;

        [SerializeField] private int quantity = 1;

        private SpriteRenderer _spriteRenderer;
        private bool _isPickedUp = false;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            if (itemData != null)
            {
                _spriteRenderer.sprite = itemData.ItemIcon;
            }
        }

        // Optionally, use OnTriggerEnter2D if you prefer automatic pickup
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if(_isPickedUp) return;
            if (collision.CompareTag("Player"))
            {
                _isPickedUp = true;
                PickupItem(collision.gameObject);
            }
        }

        // Alternatively, call this method from a player interaction script
        public void Interact(GameObject player)
        {
            PickupItem(player);
        }

        private void PickupItem(GameObject player)
        {
            if(player.TryGetComponent(out IPlayerInventoryHandler playerInventory))
            {
                if (playerInventory.GetPlayerInventory() == null)
                {
                    Debug.Log("Player inventory not found");
                    return;
                }
                if (playerInventory.GetPlayerInventory().Handle_AddItem(new InventoryItem(itemData, quantity)))
                {
                    Destroy(gameObject);
                }
                else
                {
                    _isPickedUp = false;
                }
            }
            else
            {
                _isPickedUp = false;
            }
        }
    }
}