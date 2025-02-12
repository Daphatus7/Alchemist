// Author : Peiyu Wang @ Daphatus
// 30 01 2025 01 36

namespace _Script.Quest
{
    public enum QuestState
    {
        /// <summary>
        /// The quest has not been started, it is flagged as can be accepted but not started
        /// </summary>
        NotStarted,
        /// <summary>
        /// The quest has started and the player is working on it
        /// </summary>
        InProgress,
        Completed
    }
}