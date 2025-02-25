// Author : Peiyu Wang @ Daphatus
// 07 02 2025 02 41

using System.Collections.Generic;
using _Script.Alchemy.AlchemyUI;
using _Script.Character.PlayerUI;
using _Script.Inventory.ActionBarFrontend;
using _Script.Inventory.InventoryFrontend;
using _Script.Inventory.MerchantInventoryBackend;
using _Script.Inventory.MerchantInventoryFrontend;
using _Script.NPC.NPCFrontend;
using _Script.Quest.GuildQuestUI;
using _Script.UserInterface;
using UnityEngine;

namespace _Script.Managers
{
    [DefaultExecutionOrder(-1000)]
    public class UIManager : PersistentSingleton<UIManager>
    {
        private readonly Dictionary<UIType, IUIHandler> _uiHandlers = new Dictionary<UIType, IUIHandler>(); 
        public Dictionary<UIType, IUIHandler> UIHandlers => _uiHandlers;
        
        
        [SerializeField] private InventoryManager playerInventoryUI; public InventoryManager PlayerInventoryUI => playerInventoryUI;
        [SerializeField] private MerchantInventoryUI merchantInventoryUI; public MerchantInventoryUI MerchantInventoryUI => merchantInventoryUI;
        [SerializeField] private PlayerAlchemyUI alchemyUI; public PlayerAlchemyUI AlchemyUI => alchemyUI;
        [SerializeField] private PrototypeTimer prototypeTimerUI; public PrototypeTimer PrototypeTimerUI => prototypeTimerUI;
        [SerializeField] private Prototype_Active_Quest_Ui activeQuestUI; public Prototype_Active_Quest_Ui ActiveQuestUI => activeQuestUI;
        [SerializeField] private ActionBarUI actionBarUI; public ActionBarUI ActionBarUI => actionBarUI;
        [SerializeField] private StatsDisplay statsDisplay; public StatsDisplay StatsDisplay => statsDisplay;
        [SerializeField] private GuildQuestUI guideQuestUI; public GuildQuestUI GuideQuestUI => guideQuestUI;
        [SerializeField] private NpcUi npcUI; public NpcUi NpcUI => npcUI;
        [SerializeField] private ItemDetail itemDetail; public ItemDetail ItemDetail => itemDetail;
        
    }

    public enum UIType
    {
        PlayerInventoryUI,
        AlchemyUI,
    }
}