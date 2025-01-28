// Author : Peiyu Wang @ Daphatus
// 27 01 2025 01 15

using _Script.UserInterface;
using UnityEngine;

namespace _Script.NPC.NPCFrontend
{
    public class NpcUiBase : MonoBehaviour, IUIHandler
    {
        [SerializeField] protected GameObject uiPanel;
        
        public virtual void ShowUI()
        {
            uiPanel.SetActive(true);
        }

        public virtual void HideUI()
        {
            uiPanel.SetActive(false);
        }
    }
}