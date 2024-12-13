using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.Inventory.InventoryBackend;
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
        

        public void Initialize(Collider2D col, SpriteRenderer spriteRenderer, ItemData itemData, int quantity)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
            _collider = col;
            _spriteRenderer = spriteRenderer;
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

        public virtual void Interact(GameObject player)
        {
            PickupItem(player);
        }

        public virtual void InteractEnd(GameObject player) { }

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

        protected virtual void PickupItem(GameObject player)
        {
            if (player.TryGetComponent(out PlayerCharacter playerCharacter))
            {
                // For a normal item (non-container), create a normal stack
                var stack = new ItemStack(itemData, quantity);
                if (playerCharacter.PlayerInventory.AddItem(stack) == null)
                {
                    Destroy(gameObject);
                }
                else
                {
                    Debug.Log("Not enough space in inventory!");
                }
            }
        }

    }
}