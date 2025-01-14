// Author : Peiyu Wang @ Daphatus
// 14 01 2025 01 06

using _Script.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Character.PlayerUI
{
    public class StatsDisplay : MonoBehaviour
    {
        [SerializeField] private Image healthImage;
        [SerializeField] private Image energyImage;
        [SerializeField] private Image staminaImage;
        [SerializeField] private Image hungerImage;

        private PlayerCharacter _playerCharacter;

        private void Awake()
        {
        }

        private void Start()
        {
            _playerCharacter = GameManager.Instance.GetPlayer().GetComponent<PlayerCharacter>();
            _playerCharacter.onStatsChanged.AddListener(UpdateUI);
            UpdateUI();
        }

        private void OnDestroy()
        {
            _playerCharacter.onStatsChanged.RemoveListener(UpdateUI);
        }

        
        private void UpdateUI()
        {
            SetHeathFill(_playerCharacter.Health / _playerCharacter.HealthMax);
            SetEnergyFill(_playerCharacter.Mana / _playerCharacter.ManaMax);
            SetStaminaFill(_playerCharacter.Stamina / _playerCharacter.StaminaMax);
            SetHungerFill(_playerCharacter.Food / _playerCharacter.FoodMax);
        }
        
        private void SetHeathFill(float fill)
        {
            healthImage.fillAmount = fill;
        }
        
        private void SetEnergyFill(float fill)
        {
            energyImage.fillAmount = fill;
        }
        
        private void SetStaminaFill(float fill)
        {
            staminaImage.fillAmount = fill;
        }
        
        private void SetHungerFill(float fill)
        {
            hungerImage.fillAmount = fill;
        }
        
        
        
    }
}