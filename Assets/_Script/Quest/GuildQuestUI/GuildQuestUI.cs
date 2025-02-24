// Author : Peiyu Wang @ Daphatus
// 10 02 2025 02 28

using System.Collections.Generic;
using _Script.NPC.NpcBackend.NpcModules;
using _Script.Quest.QuestInstance;
using _Script.UserInterface;
using _Script.Utilities.GenericUI;
using _Script.Utilities.ServiceLocator;
using UnityEngine;
using UnityEngine.UI;

namespace _Script.Quest.GuildQuestUI
{
    public class GuildQuestUI : MonoBehaviour, IUIHandler, IGuildQuestUIHandler
    {
        [SerializeField] private GameObject guildQuestDisplayPanel;
        [SerializeField] private GameObject guildRewardDisplayPanel;
        [SerializeField] private GameObject guildInProgressDisplayPanel;
        private Dictionary<GuildQuestUIType, GameObject> _uiPanels;
        
        [SerializeField] private LayoutGroup questDisplayLayoutGroup;
        [SerializeField] private GameObject questDisplayPrefab;
        
        // Pool of reused quest displays.
        private readonly List<GuildQuestDisplay> _questDisplays = new List<GuildQuestDisplay>();
        private IGuildQuestGiverModuleHandler _handler;
        
        private void Awake()
        {
            ServiceLocator.Instance.Register<IGuildQuestUIHandler>(this);
            _uiPanels = new Dictionary<GuildQuestUIType, GameObject>
            {
                { GuildQuestUIType.Quest, guildQuestDisplayPanel },
                { GuildQuestUIType.Reward, guildRewardDisplayPanel },
                { GuildQuestUIType.InProgress, guildInProgressDisplayPanel }
            };
            HideUI();
        }
        
        private void OnDestroy()
        {
            if (ServiceLocator.Instance != null)
                ServiceLocator.Instance.Unregister<IGuildQuestUIHandler>();
        }
        
        /// <summary>
        /// Loads quest displays by reusing existing UI objects or instantiating new ones if needed.
        /// </summary>
        private void LoadQuestDisplays(List<GuildQuestInstance> instances)
        {
            int i = 0;
            // For each quest instance, reuse an existing display or create a new one.
            for (; i < instances.Count; i++)
            {
                if (i < _questDisplays.Count)
                {
                    // Reuse the existing display.
                    var i1 = i;
                    _questDisplays[i].SetDisplay(
                        instances[i].QuestRank.ToString(),
                        instances[i].GuildQuestDefinition.questName,
                        instances[i].GuildQuestDefinition.description,
                        instances[i].GuildQuestDefinition.reward.ToString(),
                        () => OnQuestAcceptButtonClicked(instances[i1])
                    );
                    _questDisplays[i].gameObject.SetActive(true);
                }
                else
                {
                    // Create a new display if none are available.
                    AddQuestDisplay(instances[i]);
                }
            }
            // Hide any extra displays that are not used.
            for (int j = i; j < _questDisplays.Count; j++)
            {
                _questDisplays[j].gameObject.SetActive(false);
            }
        }
        
        /// <summary>
        /// Instantiates a new quest display, sets its content, and adds it to the pool.
        /// </summary>
        private void AddQuestDisplay(GuildQuestInstance instance)
        {
            var questDisplay = Instantiate(questDisplayPrefab, questDisplayLayoutGroup.transform)
                .GetComponent<GuildQuestDisplay>();

            questDisplay.SetDisplay(
                instance.QuestRank.ToString(),
                instance.GuildQuestDefinition.questName,
                instance.GuildQuestDefinition.description,
                instance.GuildQuestDefinition.reward.ToString(),
                () => OnQuestAcceptButtonClicked(instance)
            );

            _questDisplays.Add(questDisplay);
        }
        
        private void OnQuestAcceptButtonClicked(GuildQuestInstance questInstance)
        {
            _handler.OnAcceptQuest(questInstance);
            HideUI();
        }

        public void ShowUI()
        {
            Debug.Log("Showing UI");
        }

        public void LoadQuestGiver(IGuildQuestGiverModuleHandler handler)
        {
            Show(GuildQuestUIType.Quest);
            _handler = handler;
            LoadQuestDisplays(handler.GetAvailableQuests);
        }
        
        public void LoadQuestReward(GuildQuestInstance currentQuest, IGuildQuestGiverModuleHandler handler)
        {
            Show(GuildQuestUIType.Reward);
            _handler = handler;
            var rewardUI = guildRewardDisplayPanel.GetComponent<TextAndButton>();
            rewardUI.LoadUIContent(currentQuest.QuestDefinition.reward.ToString(), ConfirmReward);
        }
        
        private void ConfirmReward()
        {
            if (_handler == null)
            {
                Debug.LogError("Handler is null");
                return;
            }
            _handler.OnQuestComplete();
            QuestManager.Instance.CompleteGuildQuest();
            HideUI();
        }
        
        public void LoadQuestInProgress(GuildQuestInstance currentQuest, IGuildQuestGiverModuleHandler handler)
        {
            Show(GuildQuestUIType.InProgress);
            _handler = handler;
            var inProgressUI = guildInProgressDisplayPanel.GetComponent<TextAndButton>();
            inProgressUI.LoadUIContent("currentQuest: " + currentQuest.QuestDefinition.questName, HideUI);
        }

        private void Show(GuildQuestUIType type)
        {
            foreach (var o in _uiPanels)
            {
                o.Value.SetActive(false);
            }
            _uiPanels[type].SetActive(true);
        }

        public void HideUI()
        {
            foreach (var o in _uiPanels)
            {
                o.Value.SetActive(false);
            }
            // Optionally, clear _handler if the UI should reset its binding.
            // _handler = null;
        }
    }
    
    public interface IGuildQuestUIHandler : IGameService
    {
        void LoadQuestGiver(IGuildQuestGiverModuleHandler handler);
        void HideUI();
        void LoadQuestReward(GuildQuestInstance currentQuest, IGuildQuestGiverModuleHandler handler);
        void LoadQuestInProgress(GuildQuestInstance currentQuest, IGuildQuestGiverModuleHandler handler);
    }
    
    public enum GuildQuestUIType
    {
        Quest,
        Reward,
        InProgress
    }
}