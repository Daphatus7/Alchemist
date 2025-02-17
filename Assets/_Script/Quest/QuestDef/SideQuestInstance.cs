// Author : Peiyu Wang @ Daphatus
// 29 01 2025 01 23

namespace _Script.Quest.QuestDef
{
    public class SideQuestInstance : QuestInstance
    {
        public SideQuestInstance(QuestDefinition def) : base(def)
        {
        }

        public override QuestType QuestType => QuestType.Side;
    }
}