// Author : Peiyu Wang @ Daphatus
// 06 02 2025 02 42

using System;
using System.Collections;
using _Script.Character;
using _Script.Interactable;
using _Script.Inventory.AlchemyInventory;
using _Script.Inventory.InventoryBackend;
using _Script.Managers;
using _Script.NPC.NpcBackend;
using _Script.Utilities.ServiceLocator;
using UnityEngine;

namespace _Script.Alchemy.AlchemyTools
{
    public class AlchemyTool : NpcBase
    {
        private AlchemyContainer _container; public AlchemyContainer Container => _container;
        public BrewInstance BrewInstance { get; private set; }
        private Coroutine _brewTimer;
        
        private void Awake()
        {
            _container = new AlchemyContainer();
        }
        
        public event Action onBrewComplete;


        public bool IsEmpty => _container.IsEmpty;

        //inventory
        //当玩家与其交互
            //加载炼金台
        protected override void StartConversation(PlayerCharacter player)
        {
            base.StartConversation(player);
            if (ServiceLocator.Instance == null) 
            { throw new NullReferenceException("Service locator was not initialized"); }
            
            ServiceLocator.Instance.Get<IAlchemyUIService>().
                LoadAlchemyUI(player.PlayerAlchemy,
                player.PlayerInventory,
                this);
            ConversationInstance.AddNpcUIHandler(ServiceLocator.Instance.Get<IAlchemyUIService>().GetUIHandler());

        }
        
        public void StartBrew(BrewInstance brewInstance)
        {
            if (_brewTimer != null || BrewInstance != null)
            {
                Debug.Log("already brewing");
            }
            BrewInstance = brewInstance;
            
            //start a timer
            _brewTimer = StartCoroutine(BrewTimer(BrewInstance.BrewTime));
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
                _container.AddItem(new ItemStack(output.Data, output.Quantity));
            }
            
            BrewInstance = null;
            onBrewComplete?.Invoke();
        }

        #region Save and Load
        
        public override void LoadDefaultData()
        {
            //suppose to be empty
        }

        public override string SaveKey => "AlchemyTool";
        public override NpcSave OnSaveData()
        {
            var saveModule = new AlchemyCauldronSave
            {
  
            };
            return saveModule;
        }

        public override void OnLoadData(NpcSave data)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
    
    [Serializable]
    public class AlchemyCauldronSave : NpcSave
    {
        //TODO: Requires additional classes for inventory 
    }
}