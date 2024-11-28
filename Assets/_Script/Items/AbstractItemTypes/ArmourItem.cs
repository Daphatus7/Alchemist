using _Script.Character;
using _Script.Character.ActionStrategy;

namespace _Script.Items.AbstractItemTypes
{
    public abstract class ArmourItem : EquipmentItem
    {
        public ArmourType armourType = ArmourType.None;
        public override EquipmentType EquipmentType => EquipmentType.Armour;
        public float defence = 1;

        public override void OnSelected(PlayerCharacter playerCharacter)
        {
        }
        
        public override void OnDeselected(PlayerCharacter playerCharacter)
        {
            
        }
    }
    
    public enum ArmourType
    {
        Head,
        Chest,
        Feet,
        None,
    }
}