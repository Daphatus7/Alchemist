// Author : Peiyu Wang @ Daphatus
// 13 02 2025 02 27

using UnityEngine;

namespace _Script.Quest.QuestDefinition
{
    /// <summary>
    /// Enter this destination to complete the quest
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Area : MonoBehaviour
    {
        [SerializeField] private string areaId;
        
        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                QuestManager.Instance.OnEnteringArea(areaId);
            }
        }
    }
}