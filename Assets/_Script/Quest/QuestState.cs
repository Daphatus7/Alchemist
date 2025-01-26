// Author : Peiyu Wang @ Daphatus
// 25 01 2025 01 43

namespace _Script.Quest
{
    public class QuestState
    {
        public readonly QuestDefinition Definition;
        public bool IsActive { get; }
        public bool IsCompleted { get; }

        public QuestState(QuestDefinition def, bool isCompleted)
        {
        }
    }
}