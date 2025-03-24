
using _Script.Managers;
using _Script.Map.MapLoadContext.ContextInstance;
using _Script.Map.MapLoadContext.RewardContext;
using UnityEngine;

namespace _Script.Map.MapExit
{
    /// <summary>
    /// contains the 
    /// </summary>
    public class Gate : MonoBehaviour
    {
        private RewardType _rewardType;
        private MapLoadContextInstance _mapLoadContextInstance;


        public void Initialize(RewardType rewardType, MapLoadContextInstance mapLoadContextInstance)
        {
            _rewardType = rewardType;
            _mapLoadContextInstance = mapLoadContextInstance;
        }
        

        public void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                //let player enter the map if the gate is open and the player has pressed the button
                if (_mapLoadContextInstance.IsCompleted && Input.GetKeyDown(KeyCode.E))
                {
                    //load the new map 
                    EnterSelectedMap();
                }
            }
        }
        
        private void EnterSelectedMap()
        {
            //load the new map
            Debug.Log("Loading new map");
            GameManager.Instance.LoadSelectedScene(_mapLoadContextInstance);
        }
    }
}