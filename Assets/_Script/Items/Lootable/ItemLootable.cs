using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.Inventory.InventoryBackend;
using _Script.Inventory.ItemInstance;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items.Lootable
{
    public class ItemLootable : MonoBehaviour, IInteractable
    {
        [SerializeField] protected ItemData itemData;
        [SerializeField] protected int quantity = 1;

        protected Collider2D _collider;
        protected SpriteRenderer _spriteRenderer;

        private float verticalVelocity;
        private float gravity = -30f;
        private float height;
        private Vector3 initialPosition;
        private bool isFalling = true;
        private Vector2 horizontalVelocity;
        
        public static ItemLootable CreateLootableItem(Vector3 position, ItemData itemData, int quantity)
        {
            var obj = new GameObject(itemData.itemName);
            obj.transform.position = position;
            var lootable = obj.AddComponent<ItemLootable>();
            lootable.Initialize(obj.AddComponent<BoxCollider2D>(), obj.AddComponent<SpriteRenderer>(), itemData, quantity);
            return lootable;
        }

        public void Initialize(Collider2D col, SpriteRenderer spriteRenderer, ItemData itemData, int quantity)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            _collider = col;
            _spriteRenderer = spriteRenderer;
            _spriteRenderer.sortingLayerName = "Interactable";
            this.itemData = itemData;
            this.quantity = quantity;
            _spriteRenderer.sprite = itemData.itemIcon;
            
            _collider.isTrigger = true;
            _collider.enabled = false; 

            verticalVelocity = Random.Range(5f, 10f);
            height = 0f;
            initialPosition = transform.position;
            horizontalVelocity = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)) * 2f;

            StartCoroutine(FallEffect());
        }

        private IEnumerator FallEffect()
        {
            while(isFalling)
            {
                verticalVelocity += gravity * Time.deltaTime;
                height += verticalVelocity * Time.deltaTime;

                if (height <= 0f)
                {
                    height = 0f;
                    verticalVelocity = 0f;
                    isFalling = false;
                    _collider.enabled = true;
                }

                Vector3 position = initialPosition;
                position += (Vector3)(horizontalVelocity * Time.deltaTime);
                position.y += height;
                transform.position = position;

                float scale = Mathf.Lerp(1.2f, 1f, height / 10f);
                transform.localScale = new Vector3(scale, scale, 1f);

                yield return null;
            }

            transform.localScale = Vector3.one;
        }

        public virtual void Interact(PlayerCharacter player)
        {
            PickupItem(player);
        }
        
        public void OnHighlight()
        {
            if(_spriteRenderer != null)
                _spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f);
        }

        public void OnHighlightEnd()
        {
            if(_spriteRenderer != null)
                _spriteRenderer.color = Color.white;
        }

        protected virtual void PickupItem(PlayerCharacter player)
        {
            if (player)
            {
                // For a normal item (non-container), create a normal stack
                var stack = ItemInstanceFactory.CreateItemInstance(itemData, false, quantity);
                if (player.PlayerInventory.AddItem(stack) == null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Not enough space in inventory!");
                }
            }
        }
        
        public static GameObject DropItem(Vector3 position, ItemData itemData, int quantity)
        {
            var obj = new GameObject(itemData.itemName);
            obj.transform.position = position;
            var lootable = obj.AddComponent<ItemLootable>();
            lootable.Initialize(obj.AddComponent<BoxCollider2D>(), obj.AddComponent<SpriteRenderer>(), itemData, quantity);
            return obj;
        }
    }
}