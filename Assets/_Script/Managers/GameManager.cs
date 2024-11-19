using System;
using _Script.Character;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Managers
{
    public class GameManager : Singleton<GameManager>
    {
        //Service Locator
        private ServiceLocator _serviceLocator; public ServiceLocator GetServiceLocator() => _serviceLocator;
        
        [SerializeField] private PlayerCharacter _playerCharacter;
        public IPlayerUIHandle GetPlayerUIHandle() => _playerCharacter;
        public IPlayerInventoryHandler GetPlayerInventoryHandler() => _playerCharacter;
        protected override void Awake()
        {
            base.Awake();
            _serviceLocator = ServiceLocator.Instance;
        }
    }
}