using System;
using System.Collections.Generic;
using _Script.Interactable;
using _Script.Managers;
using UnityEngine;

namespace _Script.Alchemy.Plant
{
    /**
     * The instance of a plant.
     */
    [RequireComponent(typeof(SpriteRenderer))]
    public class Crop : MonoBehaviour, IInteractable
    {
        [SerializeField] private int _currentGrowthTime = 0; // Tracks how many days the plant has grown
        [SerializeField] private int _maturationTime = 10; // Days required for the plant to fully mature
        [SerializeField] private bool _fertilized = false; // Whether the plant has been fertilized

        [SerializeField] private List<Sprite> _growthStages; // Sprites for each growth stage
        [SerializeField] private GameObject _fruitPrefab; // Prefab of the fruit to spawn on harvest
        private SpriteRenderer _spriteRenderer; // Component to display the plant's current stage
        
        // Properties
        private bool Mature => _currentGrowthTime >= _maturationTime; // Checks if the plant is mature

        // Initialization method called when the plant is created
        public void Initialize(int maturationTime, List<Sprite> growthStages)
        {
            _maturationTime = maturationTime;
            _growthStages = growthStages;
        }

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            // Register the plant with the DayManager to handle daily updates
            if (DayManager.Instance != null)
            {
                DayManager.Instance.OnNewDay.AddListener(NewDay);
            }
        }

        private void OnDestroy()
        {
            // Unregister from the DayManager to avoid memory leaks
            if (DayManager.Instance != null)
            {
                DayManager.Instance.OnNewDay.RemoveListener(NewDay);
            }
        }

        public void Grow()
        {
            
            //can't grow if mature
            if(Mature) return;
            //increase the growth time
            _currentGrowthTime++;
            
            //get the percentage of growth
            float growthPercentage = (float) _currentGrowthTime / _maturationTime;
            
            //convert the percentage to an index
            int growthStageIndex = Mathf.FloorToInt(growthPercentage * (_growthStages.Count - 1));
            
            //set the sprite to the correct growth stage
            UpdateSprite(growthStageIndex);
        }

        private void UpdateSprite(int growthStageIndex)
        {
            _spriteRenderer.sprite = _growthStages[growthStageIndex];

        }

        public void Harvest()
        {
            if (Mature)
            {
                // Spawn the fruit prefab at the plant's position
                Instantiate(_fruitPrefab, transform.position, Quaternion.identity);

                // Optionally destroy the plant after harvesting
                Destroy(gameObject);
            }
            else
            {
                Debug.Log("Crop is not mature yet!");
            }
        }

        public void Fertilize()
        {
            if (!_fertilized)
            {
                _fertilized = true;

                // Optionally reduce maturation time as an effect of fertilization
                _maturationTime = Mathf.Max(1, _maturationTime - 1);
                Debug.Log("Crop fertilized! Maturation time reduced.");
            }
            else
            {
                Debug.Log("Crop is already fertilized!");
            }
        }

        public void NewDay()
        {
            if(Mature) return;
            _currentGrowthTime++;
            Debug.Log("Crop has grown!");
            Grow();
        }

        public void Interact(GameObject player)
        {
            if(Mature)
            {
                Harvest();
            }
        }

        public void InteractEnd(GameObject player)
        {
            
        }

        public void OnHighlight()
        {
            _spriteRenderer.color = Color.yellow;
        }
        
        public void OnHighlightEnd()
        {
            _spriteRenderer.color = Color.white;
        }
    }
}
