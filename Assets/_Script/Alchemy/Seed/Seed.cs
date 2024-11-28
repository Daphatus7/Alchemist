using _Script.Character;
using _Script.Character.ActionStrategy;
using _Script.Items.AbstractItemTypes._Script.Items;

namespace _Script.Alchemy.Seed
{
    public class Seed : ItemData
    {
        public override ItemType ItemType => ItemType.Seed;
        public int growthTime = 10;
        public int growthStage = 3;
        
        public override void Use(PlayerCharacter playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public override void OnSelected(PlayerCharacter playerCharacter)
        {
            throw new System.NotImplementedException();
        }

        public override void OnDeselected(PlayerCharacter playerCharacter)
        {
            throw new System.NotImplementedException();
        }
    }
}