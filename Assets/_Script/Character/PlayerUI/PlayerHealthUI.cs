using System.Collections.Generic;
using _Script.Character;
using _Script.Managers;
using UnityEngine;

namespace _Script.UI
{
    public class PlayerHealthUI : MonoBehaviour
    {
        // UI Components - Heart
        [SerializeField] private GameObject heartPrefab;
        [SerializeField] private float healthPerHeart = 20f; // assume each heart represents 20 health
        private List<HeartUI> hearts = new List<HeartUI>();
        private IPlayerUIHandle playerCharacter;

        private void Start()
        {
            playerCharacter = GameManager.Instance.GetPlayerUIHandle();
            playerCharacter.GetPlayerHealthUpdateEvent().AddListener(UpdateHearts);
            UpdateHearts();
        }

        private void OnDestroy()
        {
            playerCharacter.GetPlayerHealthUpdateEvent().RemoveListener(UpdateHearts);
        }

        private void UpdateHearts()
        {
            UpdateTotalHearts(playerCharacter.GetPlayerMaxHealth());
            UpdateHeartsFill(playerCharacter.GetPlayerHealth());
        }

        private void UpdateHeartsFill(float health)
        {
            // use healthPerHeart to calculate the health of each heart
            int totalHearts = hearts.Count;

            for (int i = 0; i < totalHearts; i++)
            {
                // calculate current heart health
                float heartMaxHealth = healthPerHeart;
                float currentHeartHealth = Mathf.Clamp(health - (i * healthPerHeart), 0, heartMaxHealth);
                float fillAmount = currentHeartHealth / heartMaxHealth;

                hearts[i].SetHeartFill(fillAmount);
            }
        }

        private void UpdateTotalHearts(float totalHealth)
        {
            int totalHeartsNeeded = Mathf.CeilToInt(totalHealth / healthPerHeart);

            if (totalHeartsNeeded > hearts.Count)
            {
                CreateHearts(totalHeartsNeeded - hearts.Count);
            }
            else if (totalHeartsNeeded < hearts.Count)
            {
                RemoveHearts(hearts.Count - totalHeartsNeeded);
            }
            // if totalHeartsNeeded == hearts.Count, do nothing
        }

        private void CreateHearts(int numberOfHeartsToCreate)
        {
            for (int i = 0; i < numberOfHeartsToCreate; i++)
            {
                var heart = Instantiate(heartPrefab, transform).GetComponent<HeartUI>();
                hearts.Add(heart);
            }
        }

        private void RemoveHearts(int numberOfHeartsToRemove)
        {
            for (int i = 0; i < numberOfHeartsToRemove; i++)
            {
                int lastIndex = hearts.Count - 1;
                Destroy(hearts[lastIndex].gameObject);
                hearts.RemoveAt(lastIndex);
            }
        }
    }
}
