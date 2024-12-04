using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.Inventory.InventoryBackend;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items.Lootable
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class ItemLootable : MonoBehaviour, IInteractable
    {
        [SerializeField] private ItemData itemData;
        [SerializeField] private int quantity = 1;

        private BoxCollider2D _collider;
        private SpriteRenderer _spriteRenderer;
        private bool _isPickedUp = false;

        // variables for item drop animation
        private float verticalVelocity;
        private float gravity = -30f;
        private float height;
        private Vector3 initialPosition;
        private bool isFalling = true;
        private Vector2 horizontalVelocity;

        private void Awake()
        {
            _collider = GetComponent<BoxCollider2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();

            if (itemData != null)
            {
                _spriteRenderer.sprite = itemData.ItemSprite;
            }
        }

        private void Start()
        {
            _collider.isTrigger = true;
            _collider.enabled = false; 

            verticalVelocity = Random.Range(5f, 10f);
            height = 0f;
            initialPosition = transform.position;

            // velocity for horizontal movement
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

                    //After the item has fallen, enable the collider
                    _collider.enabled = true;
                }
                
                //based on the height, move the item horizontally
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            return;
            if(_isPickedUp) return;
            if (collision.CompareTag("Player"))
            {
                _isPickedUp = true;
                PickupItem(collision.gameObject);
            }
        }

        public void Interact(GameObject player)
        {
            PickupItem(player);
        }

        public void InteractEnd(GameObject player)
        {
            
        }

        public void OnHighlight()
        {
            //light green
            _spriteRenderer.color = new Color(0.5f, 1f, 0.5f, 1f);
        }

        public void OnHighlightEnd()
        {
            if(_spriteRenderer == null) return;
            _spriteRenderer.color = Color.white;
        }

        private void PickupItem(GameObject player)
        {
            if(player.TryGetComponent(out PlayerCharacter playerCharacter))
            {
                //check if the player has an inventory
                if (playerCharacter.PlayerInventory == null)
                {
                    Debug.Log("Player inventory not found");
                    return;
                }
                
                if (playerCharacter.PlayerInventory.AddItem(new InventoryItem(itemData, quantity)) is { } item)
                {
                   Debug.Log($"Picked up {itemData.ItemName} x{quantity}");
                }
                else
                {
                    _isPickedUp = true;
                    Destroy(gameObject);
                }
            }
            else
            {
                _isPickedUp = false;
            }
        }

        public void Interact()
        {
            throw new System.NotImplementedException();
        }

        public void InteractEnd()
        {
            throw new System.NotImplementedException();
        }
    }
}