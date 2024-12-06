// Author : Peiyu Wang @ Daphatus
// 06 12 2024 12 08

using System;
using TMPro;
using UnityEngine;

namespace _Script.Character
{
    public class PrototypeUI : MonoBehaviour
    {
        public TextMeshProUGUI prototypeText;
        private PlayerCharacter _playerCharacter;

        private void Awake()
        {
            _playerCharacter = GetComponent<PlayerCharacter>();
        }

        private void Start()
        {
            _playerCharacter.onStatsChanged.AddListener(UpdateUI);
            UpdateUI();
        }

        private void OnDestroy()
        {
            _playerCharacter.onStatsChanged.RemoveListener(UpdateUI);
        }

        private void UpdateUI()
        {
            var health = (int) _playerCharacter.Health;
            var maxHealth = (int)_playerCharacter.HealthMax;
            var mana = (int)_playerCharacter.Mana;
            var maxMana = (int)_playerCharacter.ManaMax;
            var stamina = (int)_playerCharacter.Stamina;
            var maxStamina = (int)_playerCharacter.StaminaMax;
            var sanity = (int)_playerCharacter.Sanity;
            var maxSanity = (int)_playerCharacter.SanityMax;
            var hunger = (int)_playerCharacter.Hunger;
            var maxHunger = (int)_playerCharacter.HungerMax;
            prototypeText.text = $"Health: {health}/{maxHealth}\n" +
                                $"Mana: {mana}/{maxMana}\n" +
                                $"Stamina: {stamina}/{maxStamina}\n" +
                                $"Sanity: {sanity}/{maxSanity}\n" +
                                $"Hunger: {hunger}/{maxHunger}";
        }
    }
}