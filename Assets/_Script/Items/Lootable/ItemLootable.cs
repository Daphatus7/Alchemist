using System.Collections;
using _Script.Character;
using _Script.Items.AbstractItemTypes._Script.Items;
using UnityEngine;

namespace _Script.Items.Lootable
{
    [RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
    public class ItemLootable : Interactable.Interactable
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
                _spriteRenderer.sprite = itemData.ItemIcon;
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

        protected override bool CanInteract()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInteract()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInteractCanceled()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInteractCompleted()
        {
            throw new System.NotImplementedException();
        }
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
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
