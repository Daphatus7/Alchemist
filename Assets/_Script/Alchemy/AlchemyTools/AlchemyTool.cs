// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 42

using System;
using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.Inventory.AlchemyInventory;
using _Script.Inventory.InventoryBackend;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Alchemy.AlchemyTools
{
    public class AlchemyTool : MonoBehaviour, IInteractable
    {
        private AlchemyContainer _container;
        public BrewInstance BrewInstance { get; private set; }
        private Coroutine _brewTimer;
        
        public event Action onBrewComplete;


        public bool IsEmpty => _container.IsEmpty;

        //inventory
        //当玩家与其交互
            //加载炼金台
        public void Interact(PlayerCharacter player)
        {
            if (ServiceLocator.Instance == null)
            {
                throw new NullReferenceException("Service locator was not initialized");
            }
            ServiceLocator.Instance.Get<IAlchemyUIService>().LoadAlchemyUI(player.PlayerAlchemy, this);
        }

        public void OnHighlight()
        {
        }

        public void OnHighlightEnd()
        {
        }

        public void StartBrew(AlchemyRecipe selectedRecipe)
        {
            if (_brewTimer != null || BrewInstance != null)
            {
                Debug.Log("already brewing");
            }
            BrewInstance = new BrewInstance(selectedRecipe);
            
            //start a timer
            _brewTimer = StartCoroutine(BrewTimer(selectedRecipe.craftingTime));
            //when the timer is done, call OnBrewComplete
        }
        
        private IEnumerator BrewTimer(float brewTime)
        {
            yield return new WaitForSeconds(brewTime);
            OnOnBrewComplete();
        }

        protected virtual void OnOnBrewComplete()
        {
            if(BrewInstance == null)
                throw new NullReferenceException("BrewInstance is null");
            foreach (var output in BrewInstance.GetOutputItems)
            {
                _container.AddItem(new ItemStack(output.ItemData, output.Quantity));
            }
            onBrewComplete?.Invoke();
        }
    }
}