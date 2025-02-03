// Author : Peiyu Wang @ Daphatus
// 14 01 2025 01 06

using System;
using _Script.Character.PlayerAttribute;
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
            _playerCharacter.PlayerStats.OnStatsChanged += UpdateUI;
            foreach (var stat in _playerCharacter.PlayerStats.PlayerStats)
            {
                UpdateUI(stat.Key);
            }
        }
        
        

        private void OnDestroy()
        {
            _playerCharacter.PlayerStats.OnStatsChanged -= UpdateUI;
        }


        private void UpdateUI(StatType statType)
        {
            switch (statType)
            {
                case StatType.Health:
                    SetHeathFill(_playerCharacter.PlayerStats.GetStat(StatType.Health).CurrentValue /
                                 _playerCharacter.PlayerStats.GetStat(StatType.Health).MaxValue);
                    break;
                case StatType.Mana:
                    SetEnergyFill(_playerCharacter.PlayerStats.GetStat(StatType.Mana).CurrentValue /
                                  _playerCharacter.PlayerStats.GetStat(StatType.Mana).MaxValue);
                    break;
                case StatType.Stamina:
                    SetStaminaFill(_playerCharacter.PlayerStats.GetStat(StatType.Stamina).CurrentValue /
                                   _playerCharacter.PlayerStats.GetStat(StatType.Stamina).MaxValue);
                    break;
                case StatType.Food:
                    SetHungerFill(_playerCharacter.PlayerStats.GetStat(StatType.Food).CurrentValue /
                                  _playerCharacter.PlayerStats.GetStat(StatType.Food).MaxValue);
                    break;
                case StatType.Sanity:
                    // SetHungerFill(_playerCharacter.PlayerStats.GetStat(StatType.Sanity).CurrentValue /
                    //               _playerCharacter.PlayerStats.GetStat(StatType.Sanity).MaxValue);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(statType), statType, null);
            }
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